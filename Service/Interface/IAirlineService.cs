using FlightBookingCS.ViewModel;

namespace FlightBookingCS.Service.Interface;

public interface IAirlineService
{
    Task<AirlineApiResponse> GetAirlineAsync();
}