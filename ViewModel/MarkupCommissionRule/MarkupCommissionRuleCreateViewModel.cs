using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightBookingCS.ViewModel.MarkupCommissionRule;

public class MarkupCommissionRuleCreateViewModel
{
    // public string? UserId { get; set; }
    [MaxLength(5)]
    public string? AirlineCode { get; set; }

    [Required]
    [MaxLength(20)]
    public string MarkupType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal MarkupValue { get; set; }
    [Required]
    [MaxLength(20)]
    public string CommissionType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal CommissionValue { get; set; }
    public bool IsActive { get; set; }

}