using FlightBookingCS.Service.Interface;
using FlightBookingCS.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace FlightBookingCS.Service;

public class AccountService : IAccountService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;

    public AccountService(
        UserManager<IdentityUser> userManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResult> RegisterAsync(RegisterViewModel model)
    {
        // Bind model data
        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email
        };

        // Create user
        var result = await _userManager.CreateAsync(user, model.Password);

        // On Failure
        if (!result.Succeeded)
        {
            return AuthResult.Fail(result.Errors.Select(e => e.Description).ToArray());
        }

        var roles = await _userManager.GetRolesAsync(user); // Get user roles
        var token = _tokenService.GenerateToken(user.Id, user.Email!, roles); // Generate token

        return AuthResult.Success(token);
    }

    public async Task<AuthResult> LoginAsync(LoginViewModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return AuthResult.Fail("Invalid Login Attempt");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!passwordValid)
        {
            return AuthResult.Fail("Invalid Login Attempt");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user.Id, user.Email!, roles);


        return AuthResult.Success(token);
    }

}