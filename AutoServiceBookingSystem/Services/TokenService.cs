using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoServiceBookingSystem.Models;
using Microsoft.IdentityModel.Tokens;

namespace AutoServiceBookingSystem.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}

public sealed class TokenService(IConfiguration configuration) : ITokenService
{
    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is not configured.");
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var expiryMinutes = configuration.GetValue("Jwt:ExpiryMinutes", 120);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
