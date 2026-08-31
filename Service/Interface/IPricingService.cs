using FlightBookingCS.ViewModel.Flight.Responses;

namespace FlightBookingCS.Service.Interface
{
    public interface IPricingService
    {
        Task<FlightResultsViewModel> ApplyPricingToFlightsAsync(
            FlightResultsViewModel viewModel, 
            string? userId);
    }
}
