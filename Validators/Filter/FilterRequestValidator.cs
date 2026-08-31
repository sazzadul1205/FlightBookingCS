using FluentValidation;
using FlightBookingCS.ViewModel.Airline.Requests;

namespace FlightBookingCS.Validators.Filter
{
    public class FilterRequestValidator : AbstractValidator<FilterRequest>
    {
        public FilterRequestValidator()
        {
            RuleFor(x => x.IGXKey)
                .NotEmpty().WithMessage("IGXKey is required");

            RuleFor(x => x.MinPrice)
                .Must(min => !min.HasValue || min.Value >= 0)
                .WithMessage("Minimum price cannot be negative");

            RuleFor(x => x.MaxPrice)
                .Must(max => !max.HasValue || max.Value >= 0)
                .WithMessage("Maximum price cannot be negative");

            RuleFor(x => x.MaxPrice)
                .Must((request, maxPrice) =>
                    !maxPrice.HasValue ||
                    !request.MinPrice.HasValue ||
                    maxPrice.Value >= request.MinPrice.Value)
                .WithMessage("Maximum price must be greater than or equal to minimum price");
        }
    }
}
