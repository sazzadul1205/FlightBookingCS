// Service/FilterService.cs
using FlightBookingCS.Service.Interface;
using FlightBookingCS.ViewModel;
using FlightBookingCS.ViewModel.ApiModels;

namespace FlightBookingCS.Service;

public class FilterService : IFilterService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<FilterService> _logger;

    public FilterService(ICacheService cacheService, ILogger<FilterService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    // Get cached response
    public FlightSearchApiResponse? GetCachedResponse(string igxKey)
    {
        // Check if parameters are valid
        if (string.IsNullOrEmpty(igxKey)) return null;

        // Get cached data
        var cachedData = _cacheService.GetAsync(igxKey).GetAwaiter().GetResult();

        // Return
        return cachedData?.ApiResponse;
    }

    // Generate filter options
    public FilterOptions GenerateFilterOptions(List<FlightResultItem> flights)
    {
        // Initialize options
        var options = new FilterOptions();

        // Check if parameters are valid
        if (flights == null || !flights.Any()) return options;

        // Price Range
        var allFares = flights.SelectMany(f => f.FareBreakdown.Select(fb => fb.TotalFare));
        options.PriceRange.Min = allFares.Any() ? allFares.Min() : 0;
        options.PriceRange.Max = allFares.Any() ? allFares.Max() : 0;

        // Fare Types
        options.FareTypes = flights
            .Select(f => f.IsRefundable ? "Refundable" : "Non-Refundable")
            .Distinct() // Remove duplicates
            .ToList();

        // Airlines
        options.Airlines = flights
            .SelectMany(f => f.Onwards.Select(o => o.CarrierName)) // Get carrier names
            .Where(n => !string.IsNullOrEmpty(n)) // Remove empty strings
            .Distinct() // Remove duplicates
            .ToList();

        // Airline Codes
        options.AirlineCodes = flights
            .SelectMany(f => f.Onwards.Select(o => o.Carrier))
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToList();

        // Aircraft Types
        options.Aircraft = flights
            .SelectMany(f => f.Onwards.Select(o => o.Equipment))
            .Where(e => !string.IsNullOrEmpty(e))
            .Distinct()
            .ToList();

        // Baggage Options
        options.BaggageOptions = flights
            .SelectMany(f => f.Onwards.Select(o => o.AirBaggageAllowance))
            .Where(b => !string.IsNullOrEmpty(b))
            .Distinct()
            .ToList();

        // Flight Stops
        options.OnwardFlightStops = flights
            .SelectMany(f => f.TotalTravelTimes.Select(t => t.NoOfStop))
            .Distinct()
            .OrderBy(s => s)  // Order by number of stops
            .ToList();


        // Time Ranges
        options.OnwardDepartTimes = GetTimeRanges();
        options.ReturnDepartTimes = GetTimeRanges();
        options.OnwardArrivalTimes = GetTimeRanges();
        options.ReturnArrivalTimes = GetTimeRanges();

        // Duration Ranges
        options.OnwardTransitHours = GetDurationRanges();
        options.ReturnTransitHours = GetDurationRanges();
        options.OnwardFlyingTimes = GetDurationRanges();
        options.ReturnFlyingTimes = GetDurationRanges();

        // Layover Airports
        options.OnwardLayoverAirports = GetLayoverAirports(flights);
        options.ReturnLayoverAirports = options.OnwardLayoverAirports;

        // Destination Airports
        options.OnwardDestinationAirports = flights
            .SelectMany(f => f.Onwards.Select(o => o.Destination))
            .Where(d => !string.IsNullOrEmpty(d))
            .Distinct()
            .ToList();

        // Return Destination Airports
        options.ReturnDestinationAirports = options.OnwardDestinationAirports;

        return options;
    }

    // Apply filters
    public List<FlightResultItem> ApplyFilters(List<FlightResultItem> flights, FilterRequest filterRequest)
    {
        // Check if parameters are valid
        if (flights == null || !flights.Any()) return new List<FlightResultItem>();

        // Apply filters
        var result = flights.AsEnumerable();

        // Price Range Filter
        if (filterRequest.MinPrice.HasValue)
            result = result.Where(f => f.FareBreakdown.Any(fb => fb.TotalFare >= filterRequest.MinPrice.Value));

        if (filterRequest.MaxPrice.HasValue)
            result = result.Where(f => f.FareBreakdown.Any(fb => fb.TotalFare <= filterRequest.MaxPrice.Value));

        // Fare Types Filter
        if (filterRequest.FareTypes != null && filterRequest.FareTypes.Any())
        {
            result = result.Where(f =>
                (f.IsRefundable && filterRequest.FareTypes.Contains("Refundable")) ||
                (!f.IsRefundable && filterRequest.FareTypes.Contains("Non-Refundable"))
            );
        }

        // Airlines Filter
        if (filterRequest.Airlines != null && filterRequest.Airlines.Any())
        {
            result = result.Where(f =>
                f.Onwards.Any(o => filterRequest.Airlines.Contains(o.CarrierName))
            );
        }

        // Airline Codes Filter
        if (filterRequest.AirlineCodes != null && filterRequest.AirlineCodes.Any())
        {
            result = result.Where(f =>
                f.Onwards.Any(o => filterRequest.AirlineCodes.Contains(o.Carrier))
            );
        }

        // Aircraft Filter
        if (filterRequest.Aircraft != null && filterRequest.Aircraft.Any())
        {
            result = result.Where(f =>
                f.Onwards.Any(o => o.Equipment != null && filterRequest.Aircraft.Contains(o.Equipment))
            );
        }

        // Baggage Filter
        if (filterRequest.Baggage != null && filterRequest.Baggage.Any())
        {
            result = result.Where(f =>
                f.Onwards.Any(o => o.AirBaggageAllowance != null && filterRequest.Baggage.Contains(o.AirBaggageAllowance))
            );
        }

        // Onward Flight Stops Filter
        if (filterRequest.OnwardFlightStops != null && filterRequest.OnwardFlightStops.Any())
        {
            result = result.Where(f =>
                f.TotalTravelTimes.Any(t => filterRequest.OnwardFlightStops.Contains(t.NoOfStop))
            );
        }

        // Return Flight Stops Filter
        if (filterRequest.ReturnFlightStops != null && filterRequest.ReturnFlightStops.Any())
        {
            // Assuming we had a Returns property
            result = result.Where(f =>
                f.TotalTravelTimes.Any(t => filterRequest.ReturnFlightStops.Contains(t.NoOfStop))
            );
        }

        // Onward Departure Time Filter
        if (filterRequest.OnwardDepartTimes != null && filterRequest.OnwardDepartTimes.Any())
        {
            result = result.Where(f =>
                f.Onwards.Any(o => IsInTimeRange(o.DepartureTime, filterRequest.OnwardDepartTimes))
            );
        }

        // Onward Arrival Time Filter
        if (filterRequest.OnwardArrivalTimes != null && filterRequest.OnwardArrivalTimes.Any())
        {
            result = result.Where(f =>
                f.Onwards.Any(o => IsInTimeRange(o.ArrivalTime, filterRequest.OnwardArrivalTimes))
            );
        }

        // Transit Hours Filter
        if (filterRequest.OnwardTransitHours != null && filterRequest.OnwardTransitHours.Any())
        {
            result = result.Where(f =>
                f.TotalTravelTimes.Any(t =>
                    filterRequest.OnwardTransitHours.Any(th => IsInDurationRange(t.TotalTravelDuration, th))
                )
            );
        }

        // Flying Time Filter
        if (filterRequest.OnwardFlyingTimes != null && filterRequest.OnwardFlyingTimes.Any())
        {
            result = result.Where(f =>
                f.Onwards.Any(o =>
                    filterRequest.OnwardFlyingTimes.Any(ft => IsInDurationRange(o.TravelDuration, ft))
                )
            );
        }

        // Layover Airports Filter
        if (filterRequest.OnwardLayoverAirports != null && filterRequest.OnwardLayoverAirports.Any())
        {
            // For simplicity, we'll check if any segment's destination or origin matches
            result = result.Where(f =>
                f.Onwards.Any(o =>
                    filterRequest.OnwardLayoverAirports.Contains(o.Origin) ||
                    filterRequest.OnwardLayoverAirports.Contains(o.Destination)
                )
            );
        }

        // Destination Airports Filter
        if (filterRequest.OnwardDestinationAirports != null && filterRequest.OnwardDestinationAirports.Any())
        {
            result = result.Where(f =>
                f.Onwards.Any(o => filterRequest.OnwardDestinationAirports.Contains(o.Destination))
            );
        }

        return result.ToList();
    }

    // Helper methods

    // Time Range
    private List<TimeRange> GetTimeRanges()
    {
        return new List<TimeRange>
        {
            new() { Name = "00:00 To 05:59", From = TimeSpan.FromHours(0), To = TimeSpan.FromHours(5).Add(TimeSpan.FromMinutes(59)) },
            new() { Name = "06:00 To 11:59", From = TimeSpan.FromHours(6), To = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(59)) },
            new() { Name = "12:00 To 17:59", From = TimeSpan.FromHours(12), To = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(59)) },
            new() { Name = "18:00 To 23:59", From = TimeSpan.FromHours(18), To = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)) }
        };
    }

    // Duration Range
    private List<DurationRange> GetDurationRanges()
    {
        return new List<DurationRange>
        {
            new() { Name = "0 To 6 Hour", FromHours = 0, ToHours = 6 },
            new() { Name = "6 To 12 Hour", FromHours = 6, ToHours = 12 },
            new() { Name = "12 To 18 Hour", FromHours = 12, ToHours = 18 },
            new() { Name = "18 Hour +", FromHours = 18, ToHours = 99 }
        };
    }

    // Get layover airports
    private List<string> GetLayoverAirports(List<FlightResultItem> flights)
    {
        // Initialize layover airports
        var airports = new List<string>();

        // Loop through each flight
        foreach (var flight in flights)
        {
            // Loop through each segment
            for (int i = 0; i < flight.Onwards.Count - 1; i++)
            {
                // For layover, the destination of segment i is the layover point
                if (!string.IsNullOrEmpty(flight.Onwards[i].Destination))
                {
                    airports.Add(flight.Onwards[i].Destination);
                }
            }
        }
        return airports.Distinct().ToList();
    }

    // Time Range
    private bool IsInTimeRange(string timeString, List<string> timeRanges)
    {
        // Check if parameters are valid
        if (string.IsNullOrEmpty(timeString) || timeRanges == null || !timeRanges.Any()) return false;

        // Parse time
        if (!DateTime.TryParse(timeString, out var time)) return false;

        // Check if time is in range
        foreach (var rangeName in timeRanges)
        {
            // Parse range
            var range = ParseTimeRangeName(rangeName);

            // Check if time is in range
            if (range != null)
            {
                var currentTime = time.TimeOfDay;
                if (currentTime >= range.From && currentTime <= range.To)
                    return true;
            }
        }
        return false;
    }

    // Time Range
    private TimeRange? ParseTimeRangeName(string name)
    {
        var ranges = GetTimeRanges();
        return ranges.FirstOrDefault(r => r.Name == name);
    }

    // Duration Range
    private bool IsInDurationRange(string? durationString, string rangeName)
    {
        // Check if parameters are valid
        if (string.IsNullOrEmpty(durationString))
            return false;

        // Parse duration
        var hours = ParseDurationToHours(durationString);

        // Check if duration is valid
        if (!hours.HasValue) return false;

        // Check if duration is in range
        var ranges = GetDurationRanges();

        // Check if duration is in range
        var range = ranges.FirstOrDefault(r => r.Name == rangeName);

        // Check if duration is in range
        if (range == null) return false;

        return hours >= range.FromHours && hours <= range.ToHours;
    }

    // Duration Range
    private int? ParseDurationToHours(string duration)
    {
        try
        {
            // if duration contains "h" or "m"
            if (duration.Contains('h') || duration.Contains('m'))
            {
                // Split into parts
                var parts = duration.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                int totalHours = 0;

                // Loop through each part
                foreach (var part in parts)
                {
                    // Try to parse as hours
                    if (part.EndsWith('h'))
                    {
                        var value = part.Replace("h", "").Trim();
                        if (double.TryParse(value, out var hours))
                            totalHours += (int)hours;
                    }
                    else if (part.EndsWith('m')) // Try to parse as minutes
                    {
                        var value = part.Replace("m", "").Trim();
                        if (int.TryParse(value, out var minutes))
                            totalHours += minutes / 60;
                    }
                }
                return totalHours;
            }

            // Try to parse as decimal hours
            if (double.TryParse(duration, out var decimalHours)) return (int)decimalHours;
        } catch { }
        return null;
    }
}