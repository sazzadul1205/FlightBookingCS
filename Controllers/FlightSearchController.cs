using FlightBookingCS.Service.Interface;
using FlightBookingCS.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FlightBookingCS.Controllers
{
    public class FlightSearchController : Controller
    {
        private readonly IGetCitiesService _citiesService;
        private readonly IFlightSearchService _flightSearchService;
        private readonly ILogger<FlightSearchController> _logger;

        public FlightSearchController(
            ILogger<FlightSearchController> logger,
            IFlightSearchService flightSearchService,
            IGetCitiesService citiesService)
        {
            _citiesService = citiesService;
            _flightSearchService = flightSearchService;
            _logger = logger;
        }

        public ActionResult Index()
        {
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

                if (string.IsNullOrWhiteSpace(request.Origin))
                {
                    return BadRequest(new { error = "Origin is Required" });
                }

                if (string.IsNullOrWhiteSpace(request.Destination))
                {
                    return BadRequest(new { error = "Destination is Required" });
                }


                if (string.IsNullOrWhiteSpace(request.Destination))
                {
                    return BadRequest(new { error = "Destination is Required" });
                }

                if (request.NoOfAdult <= 0)
                {
                    return BadRequest(new { error = "At least one adult passenger is required" });
                }

                if (string.IsNullOrWhiteSpace(request.DepartureDate))
                {
                    return BadRequest(new { error = "Departure date is required" });
                }

                // Validate date format
                if (!DateTime.TryParse(request.DepartureDate, out _))
                {
                    return BadRequest(new { error = "Invalid departure date format. Use YYYY-MM-DD" });
                }

                // For round trip, validate return date
                if (request.JourneyType == 2 && string.IsNullOrWhiteSpace(request.ReturnDate))
                {
                    return BadRequest(new { error = "Return date is required for round trip" });
                }

                if (request.JourneyType == 2 && !DateTime.TryParse(request.ReturnDate, out _))
                {
                    return BadRequest(new { error = "Invalid return date format. Use YYYY-MM-DD" });
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
                        totalCount = 0
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Flights Found",
                    flights = result.Flights,
                    hasMore = result.HasMore,
                    totalCount = result.TotalCount
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
    }
}
