using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using FlightBookingCS.Service.Interface;

namespace FlightBookingCS.Service;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;


    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(string userId, string email, IList<string> roles)
    {
        var jwtSettings = _config.GetSection("Jwt");  // Get JWT settings
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["key"]!)); // Get JWT key
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // Get JWT signing credentials
        // Use this key and HMAC-SHA256 to sign my JWT

        // Create claims
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId), // Subject
            new Claim(JwtRegisteredClaimNames.Email, email), // Email
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID
        };

        // Add roles
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Create token
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"]!)),
            signingCredentials: creds
        );

        // Return token
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
