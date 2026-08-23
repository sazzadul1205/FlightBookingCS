using FlightBookingCS.ViewModel;

namespace FlightBookingCS.Service.Interface
{
    public interface IFlightSearchService
    {
        Task<FlightResultsViewModel> SearchFlightsAsync();
    }
}
