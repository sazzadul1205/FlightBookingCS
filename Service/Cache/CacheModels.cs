// CacheModels.cs
using FlightBookingCS.ViewModel.Flight.Requests;
using FlightBookingCS.ViewModel.Flight.Responses;

namespace FlightBookingCS.Service.Cache
{
    public class CachedFlightData
    {
        public string Id { get; set; } = string.Empty;
        public string IGXKey { get; set; } = string.Empty;
        public FlightSearchApiResponse ApiResponse { get; set; } = new();
        public FlightSearchRequest Request { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }

    public class CacheMetadata
    {
        public string IGXKey { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public long FileSize { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }

    public class CacheStatistics
    {
        public int TotalItems { get; set; }
        public int ExpiredItems { get; set; }
        public long TotalSizeInBytes { get; set; }
        public DateTime? OldestItem { get; set; }
        public DateTime? NewestItem { get; set; }
    }
}