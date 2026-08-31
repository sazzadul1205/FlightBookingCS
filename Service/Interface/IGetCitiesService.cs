using FlightBookingCS.ViewModel.Airline.Responses;

namespace FlightBookingCS.Service.Interface
{
    public interface IGetCitiesService
    {
        Task<CitiesViewModal> GetCitiesAsync(string search);
    }
}
