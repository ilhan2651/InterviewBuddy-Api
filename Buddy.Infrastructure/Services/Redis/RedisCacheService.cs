using Buddy.Application.Services;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;  // BU SATIRI EKLE
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Infrastructure.Services.Redis
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _db;
        private readonly string _prefix;
        private readonly TimeSpan _defaultTtl;

        // JSON AYARLARINI DÜZELT ✅
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,  // BU SATIRI EKLE
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public RedisCacheService(IConnectionMultiplexer mux, string prefix, int defaultTtlSeconds)
        {
            _db = mux.GetDatabase();
            _prefix = prefix;
            _defaultTtl = TimeSpan.FromSeconds(defaultTtlSeconds);
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            var val = await _db.StringGetAsync(_prefix + key);
            return val.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(val.ToString(), _json);
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
            => _db.KeyDeleteAsync(_prefix + key);

        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
        {
            var payload = JsonSerializer.Serialize(value, _json);
            await _db.StringSetAsync(_prefix + key, payload, ttl ?? _defaultTtl);
        }
    }
}