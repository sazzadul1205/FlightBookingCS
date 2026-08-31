using FlightBookingCS.ViewModel.Flight.Requests;
using FluentValidation;

namespace FlightBookingCS.Validators.Flight
{
    public class FlightSearchRequestValidator : AbstractValidator<FlightSearchRequest>
    {
        public FlightSearchRequestValidator()
        {
            RuleFor(x => x.Origin)
                .NotEmpty().WithMessage("Origin is required")
                .Length(3, 5).WithMessage("Origin must be 3-5 characters");

            RuleFor(x => x.Destination)
                .NotEmpty().WithMessage("Destination is required")
                .Length(3, 5).WithMessage("Destination must be 3-5 characters")
                .NotEqual(x => x.Origin).WithMessage("Origin and destination cannot be the same");

            RuleFor(x => x.DepartureDate)
                .NotEmpty().WithMessage("Departure date is required")
                .Must(BeAValidDate).WithMessage("Invalid departure date format")
                .Must(BeFutureDate).WithMessage("Departure date must be in the future");

            RuleFor(x => x.ReturnDate)
                .Must(BeAValidDateOrNull).WithMessage("Invalid return date format")
                .Must((request, returnDate) =>
                    string.IsNullOrEmpty(returnDate) ||
                    DateTime.Parse(returnDate) >= DateTime.Parse(request.DepartureDate))
                .WithMessage("Return date must be after departure date")
                .When(x => !string.IsNullOrEmpty(x.ReturnDate));

            RuleFor(x => x.ReturnDate)
                .NotEmpty().WithMessage("Return date is required for round trip")
                .When(x => x.JourneyType == 2);

            RuleFor(x => x.JourneyType)
                .InclusiveBetween(1, 2).WithMessage("Invalid journey type");

            RuleFor(x => x.ClassType)
                .NotEmpty().WithMessage("Class type is required")
                .Must(c => new[] { "Economy", "Business", "First" }.Contains(c))
                .WithMessage("Invalid class type");

            RuleFor(x => x.NoOfAdult)
                .GreaterThan(0).WithMessage("At least 1 adult is required")
                .LessThanOrEqualTo(9).WithMessage("Maximum 9 adults allowed");

            RuleFor(x => x.NoOfChildren)
                .GreaterThanOrEqualTo(0).WithMessage("Invalid number of children")
                .LessThanOrEqualTo(6).WithMessage("Maximum 6 children allowed");

            RuleFor(x => x.NoOfInfant)
                .GreaterThanOrEqualTo(0).WithMessage("Invalid number of infants")
                .LessThanOrEqualTo(4).WithMessage("Maximum 4 infants allowed")
                .Must((request, infants) => infants <= request.NoOfAdult)
                .WithMessage("Number of infants cannot exceed number of adults");
        }

        private bool BeAValidDate(string date)
        {
            return DateTime.TryParse(date, out _);
        }

        private bool BeAValidDateOrNull(string? date)
        {
            return string.IsNullOrEmpty(date) || DateTime.TryParse(date, out _);
        }

        private bool BeFutureDate(string date)
        {
            return DateTime.TryParse(date, out var parsedDate) && parsedDate.Date >= DateTime.UtcNow.Date;
        }
    }
}
