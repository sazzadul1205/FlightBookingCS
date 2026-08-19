using FlightBookingCS.ViewModel.MarkupCommissionRule;
using TaskManagerCS.Services;

namespace FlightBookingCS.Service.Interface;


public interface IMarkupCommissionRuleService
{
    // Task<List<MarkupCommissionRuleIndexViewModel>> GetAllAsync();
    Task<List<MarkupCommissionRuleIndexViewModel>> GetAllByUserIdAsync(string userId);
    Task<List<MarkupCommissionRuleIndexViewModel>> GetAllDeletedByUserIdAsync(string userId);
    Task<MarkupCommissionRuleIndexViewModel?> GetByIdAsync(int markupId);
    Task<ServiceResult> CreateAsync(MarkupCommissionRuleCreateViewModel model, string userId);
    Task<ServiceResult> EditAsync(MarkupCommissionRuleEditViewModel model, string userId);
    Task<ServiceResult> ChangeStatusAsync(int id, string userId);
    Task<ServiceResult> DeleteAsync(int id, string userId);
    Task<ServiceResult> RestoreAsync(int id, string userId);
    Task<ServiceResult> ForceDeleteAsync(int id, string userId);
}