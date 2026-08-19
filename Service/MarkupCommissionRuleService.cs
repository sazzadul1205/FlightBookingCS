using FlightBookingCS.Data;
using FlightBookingCS.Models;
using FlightBookingCS.Service.Interface;
using FlightBookingCS.ViewModel.MarkupCommissionRule;
using Microsoft.EntityFrameworkCore;
using TaskManagerCS.Services;

namespace FlightBookingCS.Service;

public class MarkupCommissionRuleService : IMarkupCommissionRuleService
{
    private readonly ApplicationDbContext _context;


    public MarkupCommissionRuleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MarkupCommissionRuleIndexViewModel>> GetAllByUserIdAsync(string userId)
    {
        var markups = await _context.MarkupCommissionRule.Where(x => x.UserId == userId &&
        x.DeletedAt == null).ToListAsync();

        var result = new List<MarkupCommissionRuleIndexViewModel>();

        foreach (var markup in markups)
        {
            result.Add(new MarkupCommissionRuleIndexViewModel
            {
                Id = markup.Id,
                UserId = markup.UserId,
                AirlineCode = markup.AirlineCode,
                MarkupType = markup.MarkupType,
                MarkupValue = markup.MarkupValue,
                CommissionType = markup.CommissionType,
                CommissionValue = markup.CommissionValue,
                IsActive = markup.IsActive,
                CreatedAt = markup.CreatedAt,
                UpdatedAt = markup.UpdatedAt,
            });
        }

        return result;
    }

    public async Task<List<MarkupCommissionRuleIndexViewModel>> GetAllDeletedByUserIdAsync(string userId)
    {
        var markups = await _context.MarkupCommissionRule.Where(x => x.UserId == userId &&
        x.DeletedAt != null).ToListAsync();

        var result = new List<MarkupCommissionRuleIndexViewModel>();

        foreach (var markup in markups)
        {
            result.Add(new MarkupCommissionRuleIndexViewModel
            {
                Id = markup.Id,
                UserId = markup.UserId,
                AirlineCode = markup.AirlineCode,
                MarkupType = markup.MarkupType,
                MarkupValue = markup.MarkupValue,
                CommissionType = markup.CommissionType,
                CommissionValue = markup.CommissionValue,
                IsActive = markup.IsActive,
                CreatedAt = markup.CreatedAt,
                UpdatedAt = markup.UpdatedAt,
            });
        }

        return result;
    }


    public async Task<MarkupCommissionRuleIndexViewModel?> GetByIdAsync(int markupId, string userId)
    {
        var markup = await _context.MarkupCommissionRule.FirstOrDefaultAsync(x =>
        x.Id == markupId && x.UserId == userId);

        if (markup == null)
        {
            return null;
        }

        return new MarkupCommissionRuleIndexViewModel
        {
            Id = markup.Id,
            UserId = markup.UserId,
            AirlineCode = markup.AirlineCode,
            MarkupType = markup.MarkupType,
            MarkupValue = markup.MarkupValue,
            CommissionType = markup.CommissionType,
            CommissionValue = markup.CommissionValue,
            IsActive = markup.IsActive,
            CreatedAt = markup.CreatedAt,
            UpdatedAt = markup.UpdatedAt,
        };
    }

    public async Task<ServiceResult> CreateAsync(MarkupCommissionRuleCreateViewModel model, string userId)
    {
        if (model.CommissionType == "Percentage" && (model.CommissionValue < 0 || model.CommissionValue > 100))
        {
            return ServiceResult.Error("Commission value must be between 0 and 100.");
        }

        if (model.MarkupType == "Percentage" && (model.MarkupValue < 0 || model.MarkupValue > 100))
        {
            return ServiceResult.Error("Markup value must be between 0 and 100.");
        }

        // Check if airline code Already Used.
        var checkAvailability = await _context.MarkupCommissionRule.FirstOrDefaultAsync(x =>
        x.AirlineCode == model.AirlineCode && x.UserId == userId);

        if (checkAvailability != null)
        {
            return ServiceResult.Error("Airline code Already Used..");
        }

        var markup = new MarkupCommissionRule()
        {
            UserId = userId,
            AirlineCode = model.AirlineCode,
            MarkupType = model.MarkupType,
            MarkupValue = model.MarkupValue,
            CommissionType = model.CommissionType,
            CommissionValue = model.CommissionValue,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            DeletedAt = null,
        };

        _context.MarkupCommissionRule.Add(markup);
        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> EditAsync(MarkupCommissionRuleEditViewModel model, string userId)
    {

        var result = await _context.MarkupCommissionRule
            .FirstOrDefaultAsync(x => x.Id == model.Id && x.UserId == userId);

        if (result == null)
        {
            return ServiceResult.Error("Markup not found.");
        }

        if (model.CommissionType == "Percentage" && (model.CommissionValue < 0 || model.CommissionValue > 100))
        {
            return ServiceResult.Error("Commission value must be between 0 and 100.");
        }

        if (model.MarkupType == "Percentage" && (model.MarkupValue < 0 || model.MarkupValue > 100))
        {
            return ServiceResult.Error("Markup value must be between 0 and 100.");
        }

        // Check if airline code Already Used.
        var checkAvailability = await _context.MarkupCommissionRule.FirstOrDefaultAsync(x =>
        x.AirlineCode == model.AirlineCode && x.UserId == userId);

        if (checkAvailability != null)
        {
            return ServiceResult.Error("Airline code Already Used..");
        }

        result.AirlineCode = model.AirlineCode;
        result.MarkupType = model.MarkupType;
        result.MarkupValue = model.MarkupValue;
        result.CommissionType = model.CommissionType;
        result.CommissionValue = model.CommissionValue;
        result.IsActive = model.IsActive;
        result.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ChangeStatusAsync(int id, string userId)
    {
        var result = await _context.MarkupCommissionRule
           .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (result == null)
        {
            return ServiceResult.Error("Markup not found.");
        }

        result.IsActive = !result.IsActive;

        await _context.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id, string userId)
    {
        var result = await _context.MarkupCommissionRule
           .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (result == null)
        {
            return ServiceResult.Error("Markup not found.");
        }

        result.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RestoreAsync(int id, string userId)
    {
        var result = await _context.MarkupCommissionRule
           .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (result == null)
        {
            return ServiceResult.Error("Markup not found.");
        }

        result.DeletedAt = null;

        await _context.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ForceDeleteAsync(int id, string userId)
    {
        var result = await _context.MarkupCommissionRule
           .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (result == null)
        {
            return ServiceResult.Error("Markup not found.");
        }

        _context.MarkupCommissionRule.Remove(result);

        await _context.SaveChangesAsync();

        return ServiceResult.Ok();
    }
}
