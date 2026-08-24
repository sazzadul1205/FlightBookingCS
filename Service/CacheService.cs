using FlightBookingCS.Service.Interface;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace FlightBookingCS.Service
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);
        private readonly TimeSpan _maxExpiration = TimeSpan.FromMinutes(30);


        public CacheService(
            IMemoryCache memoryCache,
            ILogger logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }


        // Get the cache
        public async Task<T?> GetTAsync<T>(string key)
        {
            try
            {
                // check if the key exists
                if (_memoryCache.TryGetValue(key, out T? value))
                {
                    _logger.LogInformation("Cache hit for key: {Key}", key);
                    return value;
                }

                _logger.LogInformation("Cache miss for key: {Key}", key);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving from cache for key: {Key}", key);
                return default;
            }

        }

        // Set the cache
        public async Task setAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                // Initialize the Expiration Time
                var cacheExpiration = expiration ?? _defaultExpiration;

                // if the expiration time is greater than the max expiration time
                if (cacheExpiration > _maxExpiration)
                {
                    cacheExpiration = _maxExpiration;
                }

                // Set the cache Options
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(cacheExpiration)      // Set the sliding expiration
                                                                // Set the absolute expiration
                    .SetAbsoluteExpiration(DateTimeOffset.UtcNow.Add(cacheExpiration))
                    .SetPriority(CacheItemPriority.Normal);     // Set the priority

                // Set the cache
                _memoryCache.Set(key, value, cacheOptions);
                _logger.LogInformation("Cached item with key: {Key}, expiration: {Expiration}",
                    key, cacheExpiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Setting cache for key: {Key}", key);
            }
        }

        // Remove the cache
        public async Task<bool> RemoveAsync(string key)
        {
            try
            {
                // Remove the cache
                _memoryCache.Remove(key);
                _logger.LogInformation("Removed cache item with key: {Key}", key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache for key: {Key}", key);
                return false;
            }
        }


        // Check if the cache exists
        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                // Check if the cache exists
                return _memoryCache.TryGetValue(key, out _);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return false;
            }
        }

        // Generate a cache key
        public async Task<string> GenerateCacheKey(string igxKey, object metadata)
        {
            // get the metadata
            var metadataJson = JsonSerializer.Serialize(metadata);

            // combine the igxKey and metadata
            var combined = $"{igxKey}_{metadataJson}";

            // create a SHA256 hash
            using var sha256 = System.Security.Cryptography.SHA256.Create();

            // convert the combined string to bytes
            var bytes = System.Text.Encoding.UTF8.GetBytes(combined);

            // calculate the hash
            var hash = sha256.ComputeHash(bytes);

            // convert the hash to a base64 string
            var key = Convert.ToBase64String(hash)
                .Replace('/', '_')
                .Replace('+', '-')
                .Substring(0, 32);

            // return the key
            return $"flight_search_{key}";

        }
    }
}
