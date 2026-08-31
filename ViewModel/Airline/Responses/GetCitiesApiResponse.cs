namespace FlightBookingCS.ViewModel.Airline.Responses
{
    public class GetCitiesApiResponse
    {
        public int ID { get; set; }
        public string AirportCode { get; set; } = string.Empty;
        public string SearchString { get; set; } = string.Empty;
        public string? CountryFlagUrl { get; set; }
    }

    public class CitiesViewModal
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<GetCitiesApiResponse> Cities { get; set; } = new();
    }
}