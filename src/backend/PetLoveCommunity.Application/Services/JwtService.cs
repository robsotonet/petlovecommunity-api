using Microsoft.IdentityModel.Tokens;
using PetLoveCommunity.Application.Configuration;
using PetLoveCommunity.Application.Interfaces;
using PetLoveCommunity.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PetLoveCommunity.Application.Services;

public class JwtService : IJwtService
{
    private readonly IJwtSettings _jwtSettings;

    public JwtService(IJwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
    }

    public string GenerateJwtToken(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("status", user.Status.ToString()),
            new("isEmailVerified", user.IsEmailVerified.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}