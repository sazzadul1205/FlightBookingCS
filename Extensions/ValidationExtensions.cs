using FluentValidation;
using FluentValidation.Results;

namespace FlightBookingCS.Extensions;

public static class ValidationExtensions
{
    public static async Task<List<string>> GetValidationErrorsAsync<T>(this IValidator<T> validator, T model)
    {
        var result = await validator.ValidateAsync(model);
        return result.Errors.Select(e => e.ErrorMessage).ToList();
    }

    public static async Task<Dictionary<string, List<string>>> GetValidationErrorsDictAsync<T>(this IValidator<T> validator, T model)
    {
        var result = await validator.ValidateAsync(model);
        return result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToList());
    }
}