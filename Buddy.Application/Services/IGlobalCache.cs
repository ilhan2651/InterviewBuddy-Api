using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Services
{
    public interface IGlobalCache
    {
        Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, bool slidingTtl = false, CancellationToken ct = default);
        
        Task<T?> GetOrSet<T>(
            string key,
            Func<CancellationToken, Task<T?>> factory,
            TimeSpan? ttl = null,
            bool slidingTtl = false,
            CancellationToken ct = default,
            Action<string>? trace = null);

        Task RemoveAsync(string key, CancellationToken ct = default);

        Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
        
        // Additional methods as per user's example
        Task ListRightPushAsync<T>(string key, T value, CancellationToken ct = default);
        Task<IReadOnlyList<T>> ListRangeAsync<T>(string key, int start = 0, int stop = -1, CancellationToken ct = default);
        Task<long> ListRemoveAsync<T>(string key, T value, CancellationToken ct = default);
        Task<IReadOnlyList<string>> ScanKeysAsync(string pattern, CancellationToken ct = default);
        Task<bool> KeyExpireAsync(string key, TimeSpan? expiry, CancellationToken ct = default);
        Task<bool> SetIfNotExistsAsync(string key, string value = "1", TimeSpan? ttl = null, CancellationToken ct = default);
        Task<bool> ExistsAsync(string key, CancellationToken ct = default);
        Task PublishAsync(string channel, string message);
        Task SubscribeAsync(string channel, Func<string, Task> handler);
    }
}
