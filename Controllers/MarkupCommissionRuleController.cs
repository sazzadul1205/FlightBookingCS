using FlightBookingCS.Service.Interface;
using FlightBookingCS.ViewModel.Airline.Responses;
using FlightBookingCS.ViewModel.MarkupCommissionRule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlightBookingCS.Controllers;

[Authorize]
public class MarkupCommissionRuleController : Controller
{
    private readonly IMarkupCommissionRuleService _markupCommissionService;
    private readonly IAirlineService _airlineService;
    private readonly IValidationService _validationService;
    private readonly ILogger<MarkupCommissionRuleController> _logger;

    public MarkupCommissionRuleController(
        IMarkupCommissionRuleService markupCommissionService,
        IAirlineService airlineService,
        IValidationService validationService,
        ILogger<MarkupCommissionRuleController> logger)
    {
        _markupCommissionService = markupCommissionService;
        _airlineService = airlineService;
        _validationService = validationService;
        _logger = logger;
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

    public async Task<IActionResult> Create(MarkupCommissionRuleCreateViewModel model)
    {
        _logger.LogInformation("Frontend Hit Create");

        if (model == null)
        {
            _logger.LogError("Model has returned Null");
            return BadRequest("Model has returned Null");
        }

        await ApplyFluentValidationAsync(model);

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
                    await LoadAirlinesToViewBag();
                    return View(model);
                }
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "An error occurred while creating the rule.";
                await LoadAirlinesToViewBag();
                return View(model);
            }
        }

        ViewBag.ErrorMessage = "Please fix the validation errors.";
        await LoadAirlinesToViewBag();
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

    public async Task<IActionResult> Edit(MarkupCommissionRuleEditViewModel model)
    {
        await ApplyFluentValidationAsync(model);

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
                    await LoadAirlinesToViewBag();
                    return View(model);
                }
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "An error occurred while updating the rule.";
                await LoadAirlinesToViewBag();
                return View(model);
            }
        }
        ViewBag.ErrorMessage = "Please fix the validation errors.";
        await LoadAirlinesToViewBag();
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

    [HttpPost, ActionName("Delete")]
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

    [HttpPost]

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

    [HttpPost]

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

    [HttpPost]

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

    private async Task LoadAirlinesToViewBag()
    {
        try
        {
            var result = await _airlineService.GetAirlineAsync();
            ViewBag.Airlines = result.Payload ?? new List<AirlinePayload>();
            ViewBag.ApiSuccess = result?.Success ?? false;
            ViewBag.ApiMessage = result?.Message ?? "Unable to fetch airlines";
        }
        catch (Exception ex)
        {
            ViewBag.Airlines = new List<AirlinePayload>();
            ViewBag.ApiSuccess = false;
            ViewBag.ApiMessage = $"Error loading airlines: {ex.Message}";
        }
    }

    private async Task ApplyFluentValidationAsync<T>(T model)
    {
        var errors = await _validationService.GetErrorsByPropertyAsync(model);

        foreach (var property in errors)
        {
            foreach (var message in property.Value)
            {
                ModelState.AddModelError(property.Key, message);
            }
        }
    }
}