using FlightBookingCS.Service.Cache;
using FlightBookingCS.ViewModel.Flight.Requests;
using FlightBookingCS.ViewModel.Flight.Responses;

namespace FlightBookingCS.Service.Interface
{
    public interface ICacheService
    {
        Task<CachedFlightData?> GetAsync(string igxKey);
        Task StoreAsyc(string igxKey, FlightSearchApiResponse apiResponse, FlightSearchRequest request);
        Task<bool> DeleteAsync(string igxKey);
        Task CleanupAsync();
        Task<int> GetCacheCountAsync();
        Task<IEnumerable<CacheMetadata>> GetCacheMetadataAsync();
    }
}
