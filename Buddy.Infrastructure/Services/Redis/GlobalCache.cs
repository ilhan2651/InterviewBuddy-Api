using Buddy.Application.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Infrastructure.Services.Redis
{
    public class GlobalCache : IGlobalCache
    {
        private readonly ILogger<GlobalCache> _logger;
        private readonly ICacheService _redis;
        private readonly IMemoryCache? _memory;
        private readonly bool _useL1;
        private readonly TimeSpan _defaultTtl;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
        private readonly string _prefix;
        private readonly IConnectionMultiplexer _mux;

        public GlobalCache(
            ICacheService redis,
            IConnectionMultiplexer mux,
            string prefix,
            IMemoryCache? memory = null,
            bool useL1 = true,
            int? defaultTtlSeconds = null,
            ILogger<GlobalCache>? logger = null)
        {
            _redis = redis;
            _mux = mux;
            _prefix = prefix ?? string.Empty;
            _memory = memory;
            _useL1 = useL1 && memory is not null;
            _defaultTtl = TimeSpan.FromSeconds(defaultTtlSeconds ?? 60);
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<GlobalCache>.Instance;
        }

        public async Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? ttl = null,
            bool slidingTtl = false,
            CancellationToken ct = default)
        {
            var effectiveTtl = ttl ?? _defaultTtl;

            // L1 (Memory)
            if (_useL1)
            {
                var opt = new MemoryCacheEntryOptions();
                if (slidingTtl) opt.SetSlidingExpiration(effectiveTtl);
                else opt.SetAbsoluteExpiration(effectiveTtl);

                _memory!.Set(key, value, opt);
            }

            // L2 (Redis)
            await _redis.SetAsync(key, value, effectiveTtl, ct);
        }

        public async Task<T?> GetOrSet<T>(
            string key,
            Func<CancellationToken, Task<T?>> factory,
            TimeSpan? ttl = null,
            bool slidingTtl = false,
            CancellationToken ct = default,
            Action<string>? trace = null)
        {
            var effectiveTtl = ttl ?? _defaultTtl;

            // L1: Memory
            if (_useL1 && _memory!.TryGetValue<T?>(key, out var l1Hit))
            {
                trace?.Invoke("L1");
                return l1Hit;
            }

            // L2: Redis
            var l2Hit = await _redis.GetAsync<T>(key, ct);
            if (l2Hit is not null)
            {
                trace?.Invoke("L2");
                if (_useL1)
                {
                    var opt = new MemoryCacheEntryOptions();
                    if (slidingTtl) opt.SetSlidingExpiration(effectiveTtl);
                    else opt.SetAbsoluteExpiration(effectiveTtl);
                    _memory!.Set(key, l2Hit, opt);
                }

                if (slidingTtl)
                {
                    await _redis.SetAsync(key, l2Hit, effectiveTtl, ct);
                }
                return l2Hit;
            }

            // Stampede prevention
            var gate = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                if (_useL1 && _memory!.TryGetValue<T?>(key, out var l1After))
                {
                    trace?.Invoke("L1");
                    return l1After;
                }

                var l2After = await _redis.GetAsync<T>(key, ct);
                if (l2After is not null)
                {
                    trace?.Invoke("L2");
                    if (_useL1)
                    {
                        var opt = new MemoryCacheEntryOptions();
                        if (slidingTtl) opt.SetSlidingExpiration(effectiveTtl);
                        else opt.SetAbsoluteExpiration(effectiveTtl);
                        _memory!.Set(key, l2After, opt);
                    }
                    return l2After;
                }

                var created = await factory(ct);
                if (created is null) return default;

                await _redis.SetAsync(key, created, effectiveTtl, ct);

                if (_useL1)
                {
                    var opt = new MemoryCacheEntryOptions();
                    if (slidingTtl) opt.SetSlidingExpiration(effectiveTtl);
                    else opt.SetAbsoluteExpiration(effectiveTtl);
                    _memory!.Set(key, created, opt);
                }

                return created;
            }
            finally
            {
                gate.Release();
                _locks.TryRemove(key, out _);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken ct = default)
        {
            if (_useL1) _memory!.Remove(key);
            await _redis.RemoveAsync(key, ct);
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            if (_useL1 && _memory!.TryGetValue<T?>(key, out var l1))
            {
                return l1;
            }

            var l2 = await _redis.GetAsync<T>(key, ct);
            if (l2 is not null && _useL1)
            {
                var opt = new MemoryCacheEntryOptions().SetAbsoluteExpiration(_defaultTtl);
                _memory!.Set(key, l2, opt);
            }
            return l2;
        }

        // Additional utility methods
        public async Task ListRightPushAsync<T>(string key, T value, CancellationToken ct = default)
        {
            var db = _mux.GetDatabase();
            var json = JsonSerializer.Serialize(value);
            var fullKey = _prefix + key;
            await db.ListRightPushAsync(fullKey, json);
        }

        public async Task<IReadOnlyList<T>> ListRangeAsync<T>(string key, int start = 0, int stop = -1, CancellationToken ct = default)
        {
            var db = _mux.GetDatabase();
            var values = await db.ListRangeAsync(_prefix + key, start, stop);
            return values
                .Where(v => v.HasValue)
                .Select(v => JsonSerializer.Deserialize<T>(v.ToString())!)
                .ToList();
        }

        public async Task<long> ListRemoveAsync<T>(string key, T value, CancellationToken ct = default)
        {
            var db = _mux.GetDatabase();
            var json = JsonSerializer.Serialize(value);
            var fullKey = _prefix + key;
            return await db.ListRemoveAsync(fullKey, json, 0);
        }

        public async Task<IReadOnlyList<string>> ScanKeysAsync(string pattern, CancellationToken ct = default)
        {
            var server = _mux.GetServer(_mux.GetEndPoints().First());
            var keys = new List<string>();
            var fullPattern = _prefix + pattern;

            await foreach (var key in server.KeysAsync(pattern: fullPattern))
            {
                if (ct.IsCancellationRequested) break;
                keys.Add(key.ToString().Replace(_prefix, ""));
            }
            return keys;
        }

        public async Task<bool> KeyExpireAsync(string key, TimeSpan? expiry, CancellationToken ct = default)
        {
            var db = _mux.GetDatabase();
            return await db.KeyExpireAsync(_prefix + key, expiry);
        }

        public async Task<bool> SetIfNotExistsAsync(string key, string value = "1", TimeSpan? ttl = null, CancellationToken ct = default)
        {
            var db = _mux.GetDatabase();
            return await db.StringSetAsync(_prefix + key, value, ttl ?? _defaultTtl, When.NotExists);
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            if (_useL1 && _memory!.TryGetValue(key, out _)) return true;
            var db = _mux.GetDatabase();
            return await db.KeyExistsAsync(_prefix + key);
        }

        public async Task PublishAsync(string channel, string message)
        {
            var sub = _mux.GetSubscriber();
            await sub.PublishAsync(RedisChannel.Literal(channel), message);
        }

        public async Task SubscribeAsync(string channel, Func<string, Task> handler)
        {
            var sub = _mux.GetSubscriber();
            await sub.SubscribeAsync(RedisChannel.Literal(channel), async (ch, msg) => await handler(msg.ToString()));
        }
    }
}
