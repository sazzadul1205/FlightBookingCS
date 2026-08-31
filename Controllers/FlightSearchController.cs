using FlightBookingCS.Service.Interface;
using FlightBookingCS.ViewModel.Airline.Requests;
using FlightBookingCS.ViewModel.Flight.Requests;
using FlightBookingCS.ViewModel.Flight.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlightBookingCS.Controllers
{
    public class FlightSearchController : Controller
    {
        private readonly IGetCitiesService _citiesService;
        private readonly IFlightSearchService _flightSearchService;
        private readonly IFilterService _filterService;
        private readonly IPricingService _pricingService;
        private readonly IValidationService _validationService;
        private readonly ILogger<FlightSearchController> _logger;

        public FlightSearchController(
            ILogger<FlightSearchController> logger,
            IFlightSearchService flightSearchService,
            IFilterService filterService,
            IPricingService pricingService,
            IGetCitiesService citiesService,
            IValidationService validationService)
        {
            _citiesService = citiesService;
            _flightSearchService = flightSearchService;
            _filterService = filterService;
            _pricingService = pricingService;
            _validationService = validationService;
            _logger = logger;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SearchResults() {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCities(string search)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(search))
                {
                    return BadRequest(new { error = "Search Term Is Required" });
                }

                var cities = await _citiesService.GetCitiesAsync(search);

                if (!cities.Success)
                {
                    return BadRequest(new { error = cities.Message });
                }
                return Json(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCities Endpoint");
                return StatusCode(500, new { error = "An error occurred while fetching cities" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetFlight([FromBody] FlightSearchRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { error = "Invalid Request" });
                }

                var validationError = await GetFirstValidationErrorAsync(request);
                if (validationError != null)
                {
                    return BadRequest(new { error = validationError });
                }

                _logger.LogInformation("Searching flights: Origin={Origin}, Destination={Destination}, Departure={DepartureDate}, Adults={NoOfAdult}",
                    request.Origin, request.Destination, request.DepartureDate, request.NoOfAdult);

                var result = await _flightSearchService.SearchFlightsAsync(request);

                if (result == null || result.Flights == null || !result.Flights.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No flights found",
                        flights = new List<FlightResultItem>(),
                        hasMore = false,
                        totalCount = 0,
                        igxKey = result?.IGXKey,
                        filterOptions = new FilterOptions(),
                    });
                }

                // Apply pricing rules for logged-in users
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var viewModel = new FlightResultsViewModel
                {
                    Flights = result.Flights,
                    HasMore = result.HasMore,
                    TotalCount = result.TotalCount,
                    IGXKey = result.IGXKey
                };
                
                viewModel = await _pricingService.ApplyPricingToFlightsAsync(viewModel, userId);

                var filterOptions = _filterService.GenerateFilterOptions(viewModel.Flights);

                return Ok(new
                {
                    success = true,
                    message = "Flights Found",
                    flights = viewModel.Flights,
                    hasMore = viewModel.HasMore,
                    totalCount = viewModel.TotalCount,
                    igxKey = viewModel.IGXKey,
                    filterOptions,
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFlight Endpoint for Origin={Origin}, Destination={Destination}",
                 request?.Origin ?? "unknown",
                 request?.Destination ?? "unknown");
                return StatusCode(500, new { error = "An error occurred while searching for flights" });
            }
        }

        [HttpPost("FlightSearch/options")]
        public async Task<IActionResult> ApplyFilter([FromBody] FilterRequest? filterRequest)
        {
            try
            {
                if (filterRequest == null)
                {
                    return BadRequest(new { error = "Invalid Request" });
                }

                var validationError = await GetFirstValidationErrorAsync(filterRequest);
                if (validationError != null)
                {
                    return BadRequest(new { error = validationError });
                }

                var cachedResponse = _filterService.GetCachedResponse(filterRequest.IGXKey);

                if (cachedResponse == null)
                {
                    return BadRequest(new { error = "No Cached Data Found, Please Search Again" });
                }

                var flights = MapPayloadToFlightResultItems(cachedResponse.Payload);
                
                // Apply pricing rules for logged-in users
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var viewModel = new FlightResultsViewModel
                {
                    Flights = flights,
                    HasMore = false,
                    TotalCount = flights.Count,
                    IGXKey = filterRequest.IGXKey
                };
                
                viewModel = _pricingService.ApplyPricingToFlightsAsync(viewModel, userId).Result;
                var filteredFlights = _filterService.ApplyFilters(viewModel.Flights, filterRequest);

                var filterOptions = _filterService.GenerateFilterOptions(filteredFlights);

                return Ok(new
                {
                    success = true,
                    flights = filteredFlights,
                    totalCount = filteredFlights.Count,
                    hasMore = false,
                    igxKey = filterRequest.IGXKey,
                    filterOptions
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filter options");
                return StatusCode(500, new { error = "An error occurred while getting filter options" });
            }
        }

        [HttpGet("FlightSearch/stats/{igxKey}")]
        public IActionResult GetFilterStats(string igxKey)
        {
            try
            {
                if (string.IsNullOrEmpty(igxKey))
                    return BadRequest(new { error = "IGXKey is required" });

                var cachedResponse = _filterService.GetCachedResponse(igxKey);
                if (cachedResponse == null)
                    return NotFound(new { error = "No cached data found for the provided IGXKey" });

                var flights = MapPayloadToFlightResultItems(cachedResponse.Payload);
                var stats = new
                {
                    totalFlights = flights.Count,
                    totalFares = flights.Sum(f => f.FareBreakdown.Sum(fb => fb.TotalFare)),
                    averageFare = flights.Any() ? flights.Average(f => f.FareBreakdown.Sum(fb => fb.TotalFare)) : 0,
                    minFare = flights.Any() ? flights.Min(f => f.FareBreakdown.Sum(fb => fb.TotalFare)) : 0,
                    maxFare = flights.Any() ? flights.Max(f => f.FareBreakdown.Sum(fb => fb.TotalFare)) : 0,
                    airlinesCount = flights.SelectMany(f => f.Onwards.Select(o => o.CarrierName)).Distinct().Count(),
                    aircraftCount = flights.SelectMany(f => f.Onwards.Select(o => o.Equipment)).Distinct().Count(),
                    statusCounts = new
                    {
                        refundable = flights.Count(f => f.IsRefundable),
                        nonRefundable = flights.Count(f => !f.IsRefundable),
                        bookable = flights.Count(f => f.IsBookable),
                        nonBookable = flights.Count(f => !f.IsBookable)
                    }
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filter stats");
                return StatusCode(500, new { error = "An error occurred while getting filter stats" });
            }
        }

        private List<FlightResultItem> MapPayloadToFlightResultItems(List<FlightSearchApiPayload>? payload)
        {
            if (payload == null || !payload.Any())
                return new List<FlightResultItem>();

            var result = new List<FlightResultItem>();

            foreach (var p in payload)
            {
                var item = new FlightResultItem
                {
                    Id = p.AirPricingSolution_Key ?? Guid.NewGuid().ToString(),
                    IsRefundable = p.IsRefundable,
                    IsBookable = p.IsBookable,
                    TripType = p.TripType,
                    PassengerType = p.PassengerType,
                    PlatingCarrierName = p.PlatingCarrierName,
                    FareType = p.FareType,

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

                    TotalTravelTimes = p.TotalTravelTimes.Select(t => new TotalTravelTimeInfo
                    {
                        TotalTravelDuration = t.TotalTravelDuration,
                        NoOfStop = t.NoOfStop
                    }).ToList(),

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

                    BrandedFareInfoes = p.BrandedFareInfoes.Select(b => new BrandedFareInfo
                    {
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
                };

                result.Add(item);
            }

            return result;
        }

        private async Task<string?> GetFirstValidationErrorAsync<T>(T model)
        {
            var errors = await _validationService.GetErrorsAsync(model);
            return errors.Count > 0 ? errors[0] : null;
        }

        private FareDetailItem? MapFareDetailItem(ApiFareDetailItem? source)
        {
            if (source == null) return null;

            return new FareDetailItem
            {
                Text = source.Text,
                OtherText = source.OtherText
            };
        }
    }
}