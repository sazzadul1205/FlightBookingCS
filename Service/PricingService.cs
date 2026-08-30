using FlightBookingCS.Data;
using FlightBookingCS.Models;
using FlightBookingCS.Service.Interface;
using FlightBookingCS.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingCS.Service
{
    public class PricingService : IPricingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PricingService> _logger;

        public PricingService(
            ApplicationDbContext context,
            ILogger<PricingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<FlightResultsViewModel> ApplyPricingToFlightsAsync(FlightResultsViewModel viewModel, string? userId)
        {
            // If user is not logged in, return original prices
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation("User not logged in - returning original prices");
                return viewModel;
            }

            // If no flights, return as is
            if (viewModel.Flights == null || !viewModel.Flights.Any())
            {
                return viewModel;
            }

            try
            {
                // Get all active markup/commission rules for this user
                var rules = await _context.MarkupCommissionRule
                    .Where(r => r.UserId == userId && r.IsActive && r.DeletedAt == null)
                    .ToListAsync();

                if (rules == null || !rules.Any())
                {
                    _logger.LogInformation("No active pricing rules found for user: {UserId}", userId);
                    return viewModel;
                }

                // Separate rules into "all airlines" and "specific airlines" - optimized with single pass
                var allAirlinesRules = new List<MarkupCommissionRule>();
                var specificAirlinesRules = new Dictionary<string, MarkupCommissionRule>();

                foreach (var rule in rules)
                {
                    if (string.IsNullOrEmpty(rule.AirlineCode))
                    {
                        allAirlinesRules.Add(rule);
                    }
                    else if (!specificAirlinesRules.ContainsKey(rule.AirlineCode))
                    {
                        specificAirlinesRules[rule.AirlineCode] = rule;
                    }
                }

                // Process each flight
                foreach (var flight in viewModel.Flights)
                {
                    // Get the airline code from the first segment
                    var airlineCode = flight.Onwards.FirstOrDefault()?.Carrier;

                    // Find matching rules for this airline
                    MarkupCommissionRule? matchingRule = null;

                    // First, try to find specific airline rules
                    if (!string.IsNullOrEmpty(airlineCode) && specificAirlinesRules.TryGetValue(airlineCode, out var specificRule))
                    {
                        matchingRule = specificRule;
                    }
                    // If no specific rule found, use "all airlines" rules
                    else if (allAirlinesRules.Any())
                    {
                        matchingRule = allAirlinesRules.First();
                    }

                    // If no matching rules found, skip this flight
                    if (matchingRule == null)
                    {
                        _logger.LogDebug("No pricing rules found for airline: {AirlineCode}", airlineCode);
                        continue;
                    }

                    // Apply pricing rule
                    ApplyPricingRule(flight, matchingRule);
                }

                _logger.LogInformation("Pricing applied to {Count} flights for user: {UserId}", viewModel.Flights.Count, userId);

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying pricing to flights for user: {UserId}", userId);
                return viewModel;
            }
        }

        private void ApplyPricingRule(FlightResultItem flight, MarkupCommissionRule rule)
        {
            // Get the first fare breakdown
            var fareBreakdown = flight.FareBreakdown.FirstOrDefault();

            // If no fare breakdown found, skip this flight
            if (fareBreakdown == null) return;

            // Initialize variables
            var baseFare = fareBreakdown.BaseFare;
            var totalTax = fareBreakdown.TotalTax;
            var baseAmmount = baseFare + totalTax;

            // Get markup amount
            var markupAmmount = CalculateMarkup(baseAmmount, rule.MarkupType, rule.MarkupValue);

            // Calculate new base fare
            var newBaseFare = baseAmmount + markupAmmount;

            // Get commission amount
            var commissionAmount = CalculateCommission(baseFare, rule.CommissionType, rule.CommissionValue);

            // Apply pricing rule
            flight.NewBaseFare = newBaseFare;

            // Set New discount
            flight.NewDiscount = commissionAmount;

            _logger.LogDebug(
                "Applied pricing rule - Airline: {Airline}, BaseFare: {BaseFare}, Tax: {Tax}, " +
                "MarkupType: {MarkupType}, MarkupValue: {MarkupValue}, CommissionType: {CommissionType}, " +
                "CommissionValue: {CommissionValue}, NewBaseFare: {NewBaseFare}, NewDiscount: {NewDiscount}",
                rule.AirlineCode ?? "All",
                baseFare,
                totalTax,
                rule.MarkupType,
                rule.MarkupValue,
                rule.CommissionType,
                rule.CommissionValue,
                newBaseFare,
                commissionAmount
            );
        }

        private decimal CalculateMarkup(decimal baseAmmount, string markupType, decimal markupValue)
        {
            if (markupType == "Percentage")
            {
                return baseAmmount * (markupValue / 100);
            }
            else
            {
                return markupValue;
            }
        }

        private decimal CalculateCommission(decimal baseFare, string commissionType, decimal commissionValue)
        {
            if (commissionType == "Percentage")
            {
                return baseFare * (commissionValue / 100);
            }
            else
            {
                return baseFare;
            }
        }

    }
}
