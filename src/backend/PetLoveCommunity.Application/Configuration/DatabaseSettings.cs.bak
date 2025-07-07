using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.Application.Configuration;

/// <summary>
/// Configuration class for database connection settings.
/// </summary>
public class DatabaseSettings : IDatabaseSettings
{
    public const string SectionName = "Database";
    
    /// <summary>
    /// The database server host address.
    /// </summary>
    [Required(ErrorMessage = "Database host is required")]
    public string Host { get; init; } = "localhost";
    
    /// <summary>
    /// The database server port number.
    /// </summary>
    [Range(1, 65535, ErrorMessage = "Database port must be between 1 and 65535")]
    public int Port { get; init; } = 5432;
    
    /// <summary>
    /// The connection timeout in seconds.
    /// </summary>
    [Range(1, 300, ErrorMessage = "Connection timeout must be between 1 and 300 seconds")]
    public int Timeout { get; init; } = 30;
    
    /// <summary>
    /// The maximum number of connections in the connection pool.
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Max pool size must be between 1 and 1000")]
    public int MaxPoolSize { get; init; } = 20;
    
    /// <summary>
    /// The minimum number of connections in the connection pool.
    /// </summary>
    [Range(0, 100, ErrorMessage = "Min pool size must be between 0 and 100")]
    public int MinPoolSize { get; init; } = 5;
    
    /// <summary>
    /// The SSL mode for the database connection.
    /// </summary>
    [Required(ErrorMessage = "SSL mode is required")]
    public string SslMode { get; init; } = "Prefer";
    
    /// <summary>
    /// Whether to include error details in connection strings.
    /// </summary>
    public bool IncludeErrorDetail { get; init; } = true;
    
    /// <summary>
    /// The command timeout in seconds.
    /// </summary>
    [Range(1, 3600, ErrorMessage = "Command timeout must be between 1 and 3600 seconds")]
    public int CommandTimeout { get; init; } = 30;
}