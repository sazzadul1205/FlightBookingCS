using FlightBookingCS.ViewModel.Airline.Requests;
using FlightBookingCS.ViewModel.Flight.Responses;

namespace FlightBookingCS.Service.Interface;

public interface IFilterService
{
    FilterOptions GenerateFilterOptions(List<FlightResultItem> flights);
    List<FlightResultItem> ApplyFilters(List<FlightResultItem> flights, FilterRequest filterRequest);
    FlightSearchApiResponse? GetCachedResponse(string igxKey);
}