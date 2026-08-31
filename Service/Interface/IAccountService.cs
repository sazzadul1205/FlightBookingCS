using FlightBookingCS.ViewModel.Auth;

namespace FlightBookingCS.Service.Interface;

public interface IAccountService
{
    Task<AuthResult> RegisterAsync(RegisterViewModel model);
    Task<AuthResult> LoginAsync(LoginViewModel model);
}