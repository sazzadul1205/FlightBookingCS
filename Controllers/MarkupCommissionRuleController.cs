using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FlightBookingCS.Service;
using FlightBookingCS.Service.Interface;
using FlightBookingCS.ViewModel.MarkupCommissionRule;

namespace FlightBookingCS.Controllers;

[Authorize]
public class MarkupCommissionRuleController : Controller
{
    private readonly IMarkupCommissionRuleService _markupCommissionService;

    public MarkupCommissionRuleController(IMarkupCommissionRuleService markupCommissionService)
    {
        _markupCommissionService = markupCommissionService;
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
    public IActionResult Create()
    {
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
}