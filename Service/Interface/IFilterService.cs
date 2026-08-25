// Service/Interface/IFilterService.cs
using FlightBookingCS.ViewModel;
using FlightBookingCS.ViewModel.ApiModels;

namespace FlightBookingCS.Service.Interface;

public interface IFilterService
{
    FilterOptions GenerateFilterOptions(List<FlightResultItem> flights);
    List<FlightResultItem> ApplyFilters(List<FlightResultItem> flights, FilterRequest filterRequest);
    FlightSearchApiResponse? GetCachedResponse(string igxKey);
}