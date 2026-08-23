using FlightBookingCS.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FlightBookingCS.Controllers
{
    public class FlightSearchController : Controller
    {
        private readonly IGetCitiesService _citiesService;
        private readonly ILogger<FlightSearchController> _logger;

        public FlightSearchController(
            ILogger<FlightSearchController> logger,
            IGetCitiesService citiesService)
        {
            _citiesService = citiesService;
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

                if (!cities.Success) {
                    return BadRequest(new { error = cities.Message });
                }
                return Json(cities);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error in GetCities Endpoint");
                return StatusCode(500, new { error = "An error occurred while fetching cities" });
            }
        }
    }
}
