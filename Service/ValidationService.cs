using FluentValidation;
using FlightBookingCS.Extensions;
using FlightBookingCS.Service.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace FlightBookingCS.Service;

public class ValidationService : IValidationService
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<List<string>> GetErrorsAsync<T>(T model)
    {
        var validator = _serviceProvider.GetService<IValidator<T>>();
        return validator is null
            ? new List<string>()
            : await validator.GetValidationErrorsAsync(model);
    }

    public async Task<Dictionary<string, List<string>>> GetErrorsByPropertyAsync<T>(T model)
    {
        var validator = _serviceProvider.GetService<IValidator<T>>();
        return validator is null
            ? new Dictionary<string, List<string>>()
            : await validator.GetValidationErrorsDictAsync(model);
    }
}