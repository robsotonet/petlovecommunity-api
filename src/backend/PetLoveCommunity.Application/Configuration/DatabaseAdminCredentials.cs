using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.Application.Configuration;

/// <summary>
/// Configuration class for database administrator credentials.
/// </summary>
public class DatabaseAdminCredentials : IDatabaseAdminCredentials
{
    public const string SectionName = "DatabaseAdmin";
    
    /// <summary>
    /// The administrator email address for database management tools.
    /// </summary>
    [Required(ErrorMessage = "Database admin email is required")]
    [EmailAddress(ErrorMessage = "Database admin email must be a valid email address")]
    public string Email { get; init; } = string.Empty;
    
    /// <summary>
    /// The administrator password for database management tools.
    /// </summary>
    [Required(ErrorMessage = "Database admin password is required")]
    [MinLength(8, ErrorMessage = "Database admin password must be at least 8 characters long")]
    public string Password { get; init; } = string.Empty;
}