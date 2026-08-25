using FlightBookingCS.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FlightBookingCS.Controllers
{
    public class CacheManagementController : Controller
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheManagementController> _logger;

        public CacheManagementController(
            ICacheService cacheService,
            ILogger<CacheManagementController> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var metadata = await _cacheService.GetCacheMetadataAsync();
            var count = await _cacheService.GetCacheCountAsync();

            ViewBag.Count = count;
            return View(metadata);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string igxKey)
        {
            try
            {
                if (string.IsNullOrEmpty(igxKey))
                {
                    return Json(new { success = false, message = "No key provided." });
                }

                var success = await _cacheService.DeleteAsync(igxKey);

                if (success)
                {
                    _logger.LogInformation($"Cache entry '{igxKey}' deleted successfully.");
                    return Json(new { success = true, message = $"Cache entry '{igxKey}' deleted." });
                }
                else
                {
                    _logger.LogWarning($"Failed to delete cache entry '{igxKey}'.");
                    return Json(new { success = false, message = $"Failed to delete entry '{igxKey}'. It may not exist." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting cache entry '{igxKey}'");
                return Json(new { success = false, message = $"An error occurred while deleting '{igxKey}'." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearAll()
        {
            try
            {
                var metadata = await _cacheService.GetCacheMetadataAsync();
                int deletedCount = 0;

                foreach (var item in metadata)
                {
                    var success = await _cacheService.DeleteAsync(item.IGXKey);
                    if (success) deletedCount++;
                }

                _logger.LogInformation($"Cleared all cache entries. Deleted {deletedCount} entries.");
                TempData["Message"] = $"All cache entries cleared. ({deletedCount} entries removed)";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all cache entries");
                TempData["Message"] = "An error occurred while clearing cache entries.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cleanup()
        {
            try
            {
                // If CleanupAsync returns void, just call it without assigning
                await _cacheService.CleanupAsync();
                _logger.LogInformation("Cleanup completed. Removed expired entries.");
                TempData["Message"] = "Cleanup completed. Removed expired entries.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cache cleanup");
                TempData["Message"] = "An error occurred during cleanup.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}