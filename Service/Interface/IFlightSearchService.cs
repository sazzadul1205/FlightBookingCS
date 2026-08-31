using FlightBookingCS.ViewModel.Flight.Requests;
using FlightBookingCS.ViewModel.Flight.Responses;

namespace FlightBookingCS.Service.Interface
{
    public interface IFlightSearchService
    {
        Task<FlightResultsViewModel> SearchFlightsAsync(FlightSearchRequest request);
    }
}
