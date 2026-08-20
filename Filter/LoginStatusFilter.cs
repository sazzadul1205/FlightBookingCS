using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FlightBookingCS.Filter;

public class LoginStatusFilter : IActionFilter
{
    // Called before the action
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Get the controller
        var controller = context.Controller as Controller;

        // Check if the controller is not null
        if (controller != null)
        {
            // Get the request
            var request = context.HttpContext.Request;

            // Get the token
            var token = request.Cookies["access_token"];

            // Set the IsLoggedIn view data
            bool isLoggedIn = false;

            // Check if the token is not empty
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    // Parse the token
                    var handler = new JwtSecurityTokenHandler();

                    // Read the token
                    var jwtToken = handler.ReadJwtToken(token);

                    // Check if the token is valid
                    if (jwtToken.ValidTo > DateTime.UtcNow)
                    {
                        isLoggedIn = true;
                    }
                }
                catch
                {
                    isLoggedIn = false;
                }
            }

            // Set the IsLoggedIn view data
            controller.ViewData["IsLoggedIn"] = isLoggedIn;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}

