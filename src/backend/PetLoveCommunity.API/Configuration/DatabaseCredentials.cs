using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.API.Configuration;

/// <summary>
/// Configuration class for database connection credentials.
/// </summary>
public class DatabaseCredentials : IDatabaseCredentials
{
    public const string SectionName = "DatabaseCredentials";
    
    /// <summary>
    /// The username for database connection.
    /// </summary>
    [Required(ErrorMessage = "Database username is required")]
    public string Username { get; init; } = string.Empty;
    
    /// <summary>
    /// The password for database connection.
    /// </summary>
    [Required(ErrorMessage = "Database password is required")]
    [MinLength(8, ErrorMessage = "Database password must be at least 8 characters long")]
    public string Password { get; init; } = string.Empty;
    
    /// <summary>
    /// The name of the database to connect to.
    /// </summary>
    [Required(ErrorMessage = "Database name is required")]
    public string DatabaseName { get; init; } = string.Empty;
}