namespace FlightBookingCS.ViewModel.Flight.Responses;

public class FlightResultsViewModel
{
    public List<FlightResultItem> Flights { get; set; } = new();
    public bool HasMore { get; set; }
    public int TotalCount { get; set; }
    public string? IGXKey { get; set; }
}

public class FlightResultItem
{
    //  IDENTIFIER 
    public string Id { get; set; } = string.Empty;

    //  FLIGHT LEVEL PROPERTIES 
    public bool IsRefundable { get; set; }
    public bool IsBookable { get; set; }
    public string? TripType { get; set; }
    public string? PassengerType { get; set; }
    public string? PlatingCarrierName { get; set; }
    public string? FareType { get; set; }

    //  NEW PRICING PROPERTIES 
    public decimal NewBaseFare { get; set; }
    public decimal NewDiscount { get; set; }

    //  ARRAYS 
    public List<FlightSegmentInfo> Onwards { get; set; } = new();
    public List<TotalTravelTimeInfo> TotalTravelTimes { get; set; } = new();
    public List<FareBreakdownInfo> FareBreakdown { get; set; } = new();
    public List<BrandedFareInfo> BrandedFareInfoes { get; set; } = new();
}
public class FlightSegmentInfo
{
    // Airline Information
    public string CarrierName { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string? OperatingCarrierName { get; set; }

    // Route
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;

    // Times
    public string DepartureTime { get; set; } = string.Empty;
    public string ArrivalTime { get; set; } = string.Empty;
    public string? TravelDuration { get; set; }

    // Additional Details
    public string? AirBaggageAllowance { get; set; }
    public string? Equipment { get; set; }
    public string? BookingCode { get; set; }
    public string? FareBasis { get; set; }
    public string? Currency { get; set; }
}
public class TotalTravelTimeInfo
{
    public string? TotalTravelDuration { get; set; }
    public int NoOfStop { get; set; }
}
public class FareBreakdownInfo
{
    public decimal TotalFare { get; set; }
    public decimal BaseFare { get; set; }
    public decimal TotalTax { get; set; }
    public decimal ApiDiscount { get; set; }
    public decimal Fees { get; set; }
    public string? PassengerType { get; set; }
    public List<TaxBreakdownInfo> TaxesBreakdown { get; set; } = new();
}
public class TaxBreakdownInfo
{
    public string? Category { get; set; }
    public decimal Amount { get; set; }
}
public class BrandedFareInfo
{
    public BrandedFareInfoDetail? BrandedFareInfoDetail { get; set; }
}
public class BrandedFareInfoDetail
{
    public FareDetailItem? CabinBaggage { get; set; }
    public FareDetailItem? CheckedBaggage { get; set; }
    public FareDetailItem? PurchaseBaggage { get; set; }
    public FareDetailItem? Change { get; set; }
    public FareDetailItem? Cancellation { get; set; }
    public FareDetailItem? Meal { get; set; }
    public FareDetailItem? SeatSelection { get; set; }
    public FareDetailItem? LoungeAccess { get; set; }
}
public class FareDetailItem
{
    public string? Text { get; set; }
    public string? OtherText { get; set; }
}

public class FilterOptions
{
    public PriceRange PriceRange { get; set; } = new();
    public List<string> FareTypes { get; set; } = new();
    public List<string> Airlines { get; set; } = new();
    public List<string> AirlineCodes { get; set; } = new();
    public List<string?> Aircraft { get; set; } = new();
    public List<string?> BaggageOptions { get; set; } = new();
    public List<int> OnwardFlightStops { get; set; } = new();
    public List<int> ReturnFlightStops { get; set; } = new();
    public List<TimeRange> OnwardDepartTimes { get; set; } = new();
    public List<TimeRange> ReturnDepartTimes { get; set; } = new();
    public List<TimeRange> OnwardArrivalTimes { get; set; } = new();
    public List<TimeRange> ReturnArrivalTimes { get; set; } = new();
    public List<DurationRange> OnwardTransitHours { get; set; } = new();
    public List<DurationRange> ReturnTransitHours { get; set; } = new();
    public List<DurationRange> OnwardFlyingTimes { get; set; } = new();
    public List<DurationRange> ReturnFlyingTimes { get; set; } = new();
    public List<string> OnwardLayoverAirports { get; set; } = new();
    public List<string> ReturnLayoverAirports { get; set; } = new();
    public List<string> OnwardDestinationAirports { get; set; } = new();
    public List<string> ReturnDestinationAirports { get; set; } = new();
}

public class PriceRange
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
}

public class TimeRange
{
    public string Name { get; set; } = string.Empty;
    public TimeSpan From { get; set; }
    public TimeSpan To { get; set; }
}

public class DurationRange
{
    public string Name { get; set; } = string.Empty;
    public int FromHours { get; set; }
    public int ToHours { get; set; }
}