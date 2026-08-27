using FlightBookingCS.ViewModel;

namespace FlightBookingCS.Service.Interface
{
    public interface IPricingService
    {
        Task<FlightResultsViewModel> ApplyPricingToFlightsAsync(
            FlightResultsViewModel viewModel, 
            string? userId);
    }
}
