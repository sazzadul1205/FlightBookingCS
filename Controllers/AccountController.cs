using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FlightBookingCS.Service.Interface;
using FlightBookingCS.ViewModel.Auth;

namespace FlightBookingCS.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IValidationService _validationService;

    public AccountController(
         IAccountService accountService,
         UserManager<IdentityUser> userManager,
         SignInManager<IdentityUser> signInManager,
         IValidationService validationService)
    {
        _accountService = accountService;
        _userManager = userManager;
        _signInManager = signInManager;
        _validationService = validationService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        await ApplyFluentValidationAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.RegisterAsync(model);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        HttpContext.Session.SetString("access_token", result.Token!);
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
        }
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        await ApplyFluentValidationAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.LoginAsync(model);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        HttpContext.Session.SetString("access_token", result.Token!);
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
        }
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Remove("access_token");
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task ApplyFluentValidationAsync<T>(T model)
    {
        var errors = await _validationService.GetErrorsByPropertyAsync(model);

        foreach (var property in errors)
        {
            foreach (var message in property.Value)
            {
                ModelState.AddModelError(property.Key, message);
            }
        }
    }

}