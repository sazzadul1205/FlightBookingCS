using FlightBookingCS.ViewModel;

namespace FlightBookingCS.Service.Interface
{
    public interface IGetCitiesService
    {
        Task<CitiesViewModal> GetCitiesAsync(string search);
    }
}
