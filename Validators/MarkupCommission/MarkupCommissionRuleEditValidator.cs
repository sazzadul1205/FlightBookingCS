using FluentValidation;
using FlightBookingCS.ViewModel.MarkupCommissionRule;

namespace FlightBookingCS.Validators.MarkupCommission
{
    public class MarkupCommissionRuleEditValidator : AbstractValidator<MarkupCommissionRuleEditViewModel>
    {
        public MarkupCommissionRuleEditValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid rule");

            RuleFor(x => x.AirlineCode)
                .MaximumLength(5).WithMessage("Airline code cannot exceed 5 characters")
                .Matches("^[A-Z]*$").WithMessage("Airline code must be uppercase letters only")
                .When(x => !string.IsNullOrEmpty(x.AirlineCode));

            RuleFor(x => x.MarkupType)
                .NotEmpty().WithMessage("Markup type is required")
                .MaximumLength(20).WithMessage("Markup type cannot exceed 20 characters")
                .Must(t => new[] { "Percentage", "Fixed" }.Contains(t))
                .WithMessage("Markup type must be 'Percentage' or 'Fixed'");

            RuleFor(x => x.MarkupValue)
                .GreaterThanOrEqualTo(0).WithMessage("Markup value must be non-negative")
                .LessThanOrEqualTo(1000000).WithMessage("Markup value cannot exceed 1,000,000");

            RuleFor(x => x.CommissionType)
                .NotEmpty().WithMessage("Commission type is required")
                .MaximumLength(20).WithMessage("Commission type cannot exceed 20 characters")
                .Must(t => new[] { "Percentage", "Fixed" }.Contains(t))
                .WithMessage("Commission type must be 'Percentage' or 'Fixed'");

            RuleFor(x => x.CommissionValue)
                .GreaterThanOrEqualTo(0).WithMessage("Commission value must be non-negative")
                .LessThanOrEqualTo(1000000).WithMessage("Commission value cannot exceed 1,000,000");

            // Custom validation: Percentage values cannot exceed 100
            When(x => x.MarkupType == "Percentage", () =>
            {
                RuleFor(x => x.MarkupValue)
                    .LessThanOrEqualTo(100).WithMessage("Percentage markup cannot exceed 100%");
            });

            When(x => x.CommissionType == "Percentage", () =>
            {
                RuleFor(x => x.CommissionValue)
                    .LessThanOrEqualTo(100).WithMessage("Percentage commission cannot exceed 100%");
            });
        }
    }
}
