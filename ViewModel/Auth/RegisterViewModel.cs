using System.ComponentModel.DataAnnotations;

namespace FlightBookingCS.ViewModel.Auth;

public class RegisterViewModel
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
}