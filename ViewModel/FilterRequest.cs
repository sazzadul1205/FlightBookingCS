namespace FlightBookingCS.ViewModel
{
    public class FilterRequest
    {
        public string IGXKey { get; set; } = string.Empty;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string>? FareTypes { get; set; }
        public List<string>? Airlines { get; set; }
        public List<string>? AirlineCodes { get; set; }
        public List<string>? Aircraft { get; set; }
        public List<string>? Baggage { get; set; }
        public List<int>? OnwardFlightStops { get; set; }
        public List<int>? ReturnFlightStops { get; set; }
        public List<string>? OnwardDepartTimes { get; set; }
        public List<string>? ReturnDepartTimes { get; set; }
        public List<string>? OnwardArrivalTimes { get; set; }
        public List<string>? ReturnArrivalTimes { get; set; }
        public List<string>? OnwardTransitHours { get; set; }
        public List<string>? ReturnTransitHours { get; set; }
        public List<string>? OnwardFlyingTimes { get; set; }
        public List<string>? ReturnFlyingTimes { get; set; }
        public List<string>? OnwardLayoverAirports { get; set; }
        public List<string>? ReturnLayoverAirports { get; set; }
        public List<string>? OnwardDestinationAirports { get; set; }
        public List<string>? ReturnDestinationAirports { get; set; }
    }
}
