namespace FlightBookingCS.ViewModel.MarkupCommissionRule;

public class MarkupCommissionRuleIndexViewModel
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? AirlineCode { get; set; }
    public string? AirlineName { get; set; }
    public string MarkupType { get; set; } = string.Empty;
    public decimal MarkupValue { get; set; }
    public string CommissionType { get; set; } = string.Empty;
    public decimal CommissionValue { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

}