using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FlightBookingCS.Models;
using System.IdentityModel.Tokens.Jwt;

namespace FlightBookingCS.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var token = Request.Cookies["access_token"];
        bool isLoggedIn = false;
        string userEmail = "";

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Check if token hasn't expired
                if (jwtToken.ValidTo > DateTime.UtcNow)
                {
                    isLoggedIn = true;

                    // Extract user info from claims
                    var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
                    userEmail = emailClaim?.Value ?? "";
                    ViewData["jwtToken"] = jwtToken;
                }
            }
            catch
            {
                // Invalid token
                isLoggedIn = false;
                Response.Cookies.Delete("access_token");
            }
        }

        // Pass the login status to the view
        ViewData["IsLoggedIn"] = isLoggedIn;
        ViewData["UserEmail"] = userEmail;
        ViewData["Token"] = token;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}