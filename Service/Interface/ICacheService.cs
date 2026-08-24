namespace FlightBookingCS.Service.Interface
{
    public interface ICacheService
    {
        Task<T?> GetTAsync<T>(string key);
        Task setAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task<bool> RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<string> GenerateCacheKey(string igxKey, object metadata);

    }
}
