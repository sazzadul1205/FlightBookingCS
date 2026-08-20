using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace FlightBookingCS.Filter;

public class SessionJwtAuthFilter : IActionFilter
{
    private readonly IConfiguration _config;

    public SessionJwtAuthFilter(IConfiguration config)
    {
        _config = config;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Get the session
        var session = context.HttpContext.Session;

        // Get the token
        var token = session.GetString("access_token");

        // Set the IsLoggedIn view data
        bool isLoggedIn = false;

        // Set the UserEmail view data
        string userEmail = "";

        // Check if the token is not empty
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                // Parse the token
                var handler = new JwtSecurityTokenHandler();

                // Read the token
                var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT signing key is not configured.");

                // Get JWT key
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

                // Validate the token (checks signature, issuer, audience, lifetime)
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out _);

                isLoggedIn = true;

                // Extract email claim
                var emailClaim = principal.FindFirst(ClaimTypes.Email) ?? principal.FindFirst("email");

                // Get email
                userEmail = emailClaim?.Value ?? "";

                // Set the user
                context.HttpContext.User = principal;
            }
            catch
            {
                // Token invalid – remove it from session
                session.Remove("access_token");
                isLoggedIn = false;
            }
        }

        // Set ViewData for all controllers/views
        if (context.Controller is Controller controller)
        {
            controller.ViewData["IsLoggedIn"] = isLoggedIn;
            controller.ViewData["UserEmail"] = userEmail;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}