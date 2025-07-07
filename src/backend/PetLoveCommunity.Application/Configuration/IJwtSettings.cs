using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.Application.Configuration;

/// <summary>
/// Configuration interface for JWT authentication settings.
/// </summary>
public interface IJwtSettings
{
    /// <summary>
    /// The secret key used for signing JWT tokens.
    /// </summary>
    string Key { get; }
    
    /// <summary>
    /// The issuer of the JWT tokens.
    /// </summary>
    string Issuer { get; }
    
    /// <summary>
    /// The intended audience for the JWT tokens.
    /// </summary>
    string Audience { get; }
    
    /// <summary>
    /// The number of hours before the JWT token expires.
    /// </summary>
    int ExpirationHours { get; }
}