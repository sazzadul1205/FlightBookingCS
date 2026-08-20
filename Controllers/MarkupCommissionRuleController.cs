using FlightBookingCS.Service.Interface;
using FlightBookingCS.ViewModel;
using FlightBookingCS.ViewModel.MarkupCommissionRule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlightBookingCS.Controllers;

[Authorize]
public class MarkupCommissionRuleController : Controller
{
    private readonly IMarkupCommissionRuleService _markupCommissionService;
    private readonly IAirlineService _airlineService;

    public MarkupCommissionRuleController(
        IMarkupCommissionRuleService markupCommissionService,
        IAirlineService airlineService)
    {
        _markupCommissionService = markupCommissionService;
        _airlineService = airlineService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _markupCommissionService.GetAllByUserIdAsync(userId!);
            return View(result);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while loading the rules.";
            return View(new List<MarkupCommissionRuleIndexViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> DeletedIndex()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _markupCommissionService.GetAllDeletedByUserIdAsync(userId!);
            return View(result);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while loading deleted rules.";
            return View(new List<MarkupCommissionRuleIndexViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int Id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _markupCommissionService.GetByIdAsync(Id, userId!);

            if (result == null)
            {
                TempData["ErrorMessage"] = "Rule not found.";
                return RedirectToAction("Index");
            }

            return View(result);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while loading the rule details.";
            return RedirectToAction("Index");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadAirlinesToViewBag();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MarkupCommissionRuleCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _markupCommissionService.CreateAsync(model, userId!);


                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Rule created successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ErrorMessage = result.ErrorMessage;
                    return View(model);
                }
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "An error occurred while creating the rule.";
                return View(model);
            }
        }
        ViewBag.ErrorMessage = "Please fix the validation errors.";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int Id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _markupCommissionService.GetByIdAsync(Id, userId!);

            if (result == null)
            {
                TempData["ErrorMessage"] = "Rule not found.";
                return RedirectToAction("Index");
            }

            await LoadAirlinesToViewBag();

            var model = new MarkupCommissionRuleEditViewModel
            {
                Id = result.Id,
                UserId = result.UserId,
                AirlineCode = result.AirlineCode,
                MarkupType = result.MarkupType,
                MarkupValue = result.MarkupValue,
                CommissionType = result.CommissionType,
                CommissionValue = result.CommissionValue,
                IsActive = result.IsActive
            };

            return View(model);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while loading the rule for editing.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MarkupCommissionRuleEditViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _markupCommissionService.EditAsync(model, userId!);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Rule updated successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ErrorMessage = result.ErrorMessage;
                    return View(model);
                }
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "An error occurred while updating the rule.";
                return View(model);
            }
        }
        ViewBag.ErrorMessage = "Please fix the validation errors.";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int Id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _markupCommissionService.GetByIdAsync(Id, userId!);

            if (result == null)
            {
                TempData["ErrorMessage"] = "Rule not found.";
                return RedirectToAction("Index");
            }

            return View(result);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while loading the rule for deletion.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost, ActionName("ChangeStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int Id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _markupCommissionService.ChangeStatusAsync(Id, userId!);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Rule status changed successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while changing the rule status.";
        }
        return RedirectToAction("Index");
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int Id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _markupCommissionService.DeleteAsync(Id, userId!);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Rule deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while deleting the rule.";
        }
        return RedirectToAction("Index");
    }

    [HttpPost, ActionName("Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int Id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _markupCommissionService.RestoreAsync(Id, userId!);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Rule restored successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while restoring the rule.";
        }
        return RedirectToAction("DeletedIndex");
    }

    [HttpPost, ActionName("ForceDelete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForceDelete(int Id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _markupCommissionService.ForceDeleteAsync(Id, userId!);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Rule permanently deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while permanently deleting the rule.";
        }
        return RedirectToAction("DeletedIndex");
    }

    //[HttpGet]
    //public async Task<IActionResult> GetAirlineAsync()
    //{
    //    var result = await _airlineService.GetAirlineAsync();

    //    if (result.Success)
    //    {
    //        var airlineSelectedList = result.Airlines.Select(a => new
    //        {
    //            code = a.Code,
    //            name = a.AriLineName,
    //            id = a.ID
    //        }).ToList();

    //        return Json(new { success = true, airlines = airlineSelectedList });
    //    }
    //    return Json(new { success = false, message = result.Message });
    //}

    private async Task LoadAirlinesToViewBag()
    {
        try {
            var result = await _airlineService.GetAirlineAsync();
            ViewBag.Airlines = result.Airlines ?? new List<AirlinePayload>();
            ViewBag.ApiSuccess = result?.Success ?? false;
            ViewBag.ApiMessage = result?.Message ?? "Unable to fetch airlines";
        } catch(Exception ex) {
            ViewBag.Airlines = new List<AirlinePayload>();
            ViewBag.ApiSuccess = false;
            ViewBag.ApiMessage = $"Error loading airlines: {ex.Message}";
        }
    }

    [HttpGet]
    public async Task<IActionResult> DebugApi()
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "FlightBookingApp/1.0");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

            var response = await client.GetAsync("https://uthaotrip.com/api/api/GetAirLines");

            // Read as byte array first
            var bytes = await response.Content.ReadAsByteArrayAsync();

            // Check if it's GZIP compressed (starts with 0x1F 0x8B)
            bool isGzip = bytes.Length > 2 && bytes[0] == 0x1F && bytes[1] == 0x8B;

            string decompressedContent = "";
            string rawContent = "";

            if (isGzip)
            {
                // Decompress GZIP
                using var inputStream = new MemoryStream(bytes);
                using var gzipStream = new System.IO.Compression.GZipStream(inputStream, System.IO.Compression.CompressionMode.Decompress);
                using var outputStream = new MemoryStream();
                gzipStream.CopyTo(outputStream);
                var decompressedBytes = outputStream.ToArray();
                decompressedContent = System.Text.Encoding.UTF8.GetString(decompressedBytes);
                rawContent = decompressedContent;
            }
            else
            {
                // Not compressed, convert directly
                rawContent = System.Text.Encoding.UTF8.GetString(bytes);
            }

            var result = new
            {
                StatusCode = (int)response.StatusCode,
                StatusDescription = response.ReasonPhrase,
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "Unknown",
                ContentLength = response.Content.Headers.ContentLength,
                ContentEncoding = response.Content.Headers.ContentEncoding?.FirstOrDefault() ?? "None",
                IsSuccess = response.IsSuccessStatusCode,
                IsGzipCompressed = isGzip,
                DecompressedContent = decompressedContent,
                RawContent = rawContent,
                Bytes = bytes
            };

            // Display first few bytes in hex for debugging
            var hexPreview = string.Join(" ", result.Bytes.Take(50).Select(b => b.ToString("X2")));

            ViewBag.StatusCode = result.StatusCode;
            ViewBag.StatusDescription = result.StatusDescription;
            ViewBag.ContentType = result.ContentType;
            ViewBag.ContentEncoding = result.ContentEncoding;
            ViewBag.ContentLength = result.ContentLength;
            ViewBag.IsSuccess = result.IsSuccess;
            ViewBag.IsGzipCompressed = result.IsGzipCompressed;
            ViewBag.RawContent = result.RawContent.Length > 1000 ? result.RawContent.Substring(0, 1000) + "..." : result.RawContent;
            ViewBag.FullContent = result.RawContent;
            ViewBag.HexPreview = hexPreview;
            ViewBag.BytesCount = result.Bytes.Length;

            return View();
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            ViewBag.StackTrace = ex.StackTrace;
            return View();
        }
    }
}