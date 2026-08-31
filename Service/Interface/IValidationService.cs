namespace FlightBookingCS.Service.Interface;

public interface IValidationService
{
    Task<List<string>> GetErrorsAsync<T>(T model);
    Task<Dictionary<string, List<string>>> GetErrorsByPropertyAsync<T>(T model);
}