using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.API.Configuration;

/// <summary>
/// Configuration class for JWT authentication settings.
/// </summary>
public class JwtSettings : IJwtSettings
{
    public const string SectionName = "Jwt";
    
    /// <summary>
    /// The secret key used for signing JWT tokens.
    /// </summary>
    [Required(ErrorMessage = "JWT Key is required")]
    [MinLength(32, ErrorMessage = "JWT Key must be at least 32 characters long")]
    public string Key { get; init; } = string.Empty;
    
    /// <summary>
    /// The issuer of the JWT tokens.
    /// </summary>
    [Required(ErrorMessage = "JWT Issuer is required")]
    public string Issuer { get; init; } = string.Empty;
    
    /// <summary>
    /// The intended audience for the JWT tokens.
    /// </summary>
    [Required(ErrorMessage = "JWT Audience is required")]
    public string Audience { get; init; } = string.Empty;
    
    /// <summary>
    /// The number of hours before the JWT token expires.
    /// </summary>
    [Range(1, 168, ErrorMessage = "JWT expiration must be between 1 and 168 hours (1 week)")]
    public int ExpirationHours { get; init; } = 24;
}