using FlightBookingCS.Service.Interface;
using FlightBookingCS.ViewModel;
using FlightBookingCS.ViewModel.ApiModels;

namespace FlightBookingCS.Service
{
    public class FlightSearchService : IFlightSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AirlineService> _logger;

        public FlightSearchService(HttpClient httpClient, ILogger<AirlineService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<FlightResultsViewModel> SearchFlightsAsync(FlightSearchRequest request)
        {

        }

        // Map FlightSearchApiResponse to FlightResultsViewModel
        private FlightResultsViewModel MapToViewModel(FlightSearchApiResponse apiResponse)
        {
            // Check if the API call is not successful
            if (!apiResponse.Success ||     // API call is not successful
            apiResponse.Payload == null ||  // Payload is null
            apiResponse.Payload.Count == 0)     // Payload is empty
            {
                // Return empty FlightResultsViewModel
                return new FlightResultsViewModel
                {
                    Flights = new List<FlightResultItem>(),
                    HasMore = false,
                    TotalCount = 0
                };
            }

            // Map API response to FlightResultsViewModel
            return new FlightResultsViewModel
            {
                Flights = apiResponse.Payload.Select(p => new FlightResultItem
                {
                    Id = p.AirPricingSolution_Key ?? Guid.NewGuid().ToString(),
                    IsRefundable = p.IsRefundable,
                    IsBookable = p.IsBookable,
                    TripType = p.TripType,
                    PassengerType = p.PassengerType,
                    PlatingCarrierName = p.PlatingCarrierName,
                    FareType = p.FareType,

                    // Map Onwards to FlightSegmentInfo
                    Onwards = p.Onwards.Select(s => new FlightSegmentInfo
                    {
                        CarrierName = s.CarrierName ?? string.Empty,
                        Carrier = s.Carrier ?? string.Empty,
                        FlightNumber = s.FlightNumber ?? string.Empty,
                        OperatingCarrierName = s.OperatingCarrierName,
                        Origin = s.Origin ?? string.Empty,
                        Destination = s.Destination ?? string.Empty,
                        DepartureTime = s.DepartureTime ?? string.Empty,
                        ArrivalTime = s.ArrivalTime ?? string.Empty,
                        TravelDuration = s.TravelDuration,
                        AirBaggageAllowance = s.AirBaggageAllowance,
                        Equipment = s.Equipment,
                        BookingCode = s.BookingCode,
                        FareBasis = s.FareBasis,
                        Currency = s.Currency
                    }).ToList(),

                    // Map Return to FlightSegmentInfo
                    TotalTravelTimes = p.TotalTravelTimes.Select(t => new TotalTravelTimeInfo
                    {
                        TotalTravelDuration = t.TotalTravelDuration,
                        NoOfStop = t.NoOfStop
                    }).ToList(),

                    // Map FareBreakdown
                    FareBreakdown = p.FareBreakdown.Select(f => new FareBreakdownInfo
                    {
                        TotalFare = f.TotalFare,
                        BaseFare = f.BaseFare,
                        TotalTax = f.TotalTax,
                        ApiDiscount = f.ApiDiscount,
                        Fees = f.Fees,
                        PassengerType = f.PassengerType ?? p.PassengerType,
                        TaxesBreakdown = f.TaxesBreakdown.Select(t => new TaxBreakdownInfo
                        {
                            Category = t.Category,
                            Amount = t.Amount
                        }).ToList()
                    }).ToList(),

                    // Map BrandedFareInfo
                    BrandedFareInfoes = p.BrandedFareInfoes.Select(b => new BrandedFareInfo
                    {
                        // Map BrandedFareInfoDetail
                        BrandedFareInfoDetail = b.BrandedFareInfoDetail != null ? new BrandedFareInfoDetail
                        {
                            CabinBaggage = MapFareDetailItem(b.BrandedFareInfoDetail.CabinBaggage),
                            CheckedBaggage = MapFareDetailItem(b.BrandedFareInfoDetail.CheckedBaggage),
                            PurchaseBaggage = MapFareDetailItem(b.BrandedFareInfoDetail.PurchaseBaggage),
                            Change = MapFareDetailItem(b.BrandedFareInfoDetail.Change),
                            Cancellation = MapFareDetailItem(b.BrandedFareInfoDetail.Cancellation),
                            Meal = MapFareDetailItem(b.BrandedFareInfoDetail.Meal),
                            SeatSelection = MapFareDetailItem(b.BrandedFareInfoDetail.SeatSelection),
                            LoungeAccess = MapFareDetailItem(b.BrandedFareInfoDetail.LoungeAccess)
                        } : null
                    }).ToList()
                }).ToList(),

                HasMore = !apiResponse.IsComplete,
                TotalCount = apiResponse.TotalVolume
            };
        }

        // Map ApiFareDetailItem to FareDetailItem
        private FareDetailItem? MapFareDetailItem(ApiFareDetailItem? source)
        {
            if (source == null)
            {
                return null;
            }

            return new FareDetailItem
            {
                Text = source.Text,
                OtherText = source.OtherText,
            };
        }

        // Map ErrorResponse to FlightResultsViewModel
        private FlightResultsViewModel CreateErrorViewModel(string Message)
        {
            _logger.LogError("Flight Search Error: {Message}", Message);
            return new FlightResultsViewModel
            {
                Flights = new List<FlightResultItem>(),
                HasMore = false,
                TotalCount = 0
            };
        }
    }
}
