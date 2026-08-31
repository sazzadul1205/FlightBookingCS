using FlightBookingCS.ViewModel.Airline.Responses;

namespace FlightBookingCS.Service.Interface;

public interface IAirlineService
{
    Task<AirlineApiResponse> GetAirlineAsync();
}