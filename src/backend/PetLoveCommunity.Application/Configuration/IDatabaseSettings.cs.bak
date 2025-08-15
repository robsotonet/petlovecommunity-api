namespace PetLoveCommunity.API.Configuration;

/// <summary>
/// Configuration interface for database connection settings.
/// </summary>
public interface IDatabaseSettings
{
    /// <summary>
    /// The database server host address.
    /// </summary>
    string Host { get; }
    
    /// <summary>
    /// The database server port number.
    /// </summary>
    int Port { get; }
    
    /// <summary>
    /// The connection timeout in seconds.
    /// </summary>
    int Timeout { get; }
    
    /// <summary>
    /// The maximum number of connections in the connection pool.
    /// </summary>
    int MaxPoolSize { get; }
    
    /// <summary>
    /// The minimum number of connections in the connection pool.
    /// </summary>
    int MinPoolSize { get; }
    
    /// <summary>
    /// The SSL mode for the database connection.
    /// </summary>
    string SslMode { get; }
    
    /// <summary>
    /// Whether to include error details in connection strings.
    /// </summary>
    bool IncludeErrorDetail { get; }
    
    /// <summary>
    /// The command timeout in seconds.
    /// </summary>
    int CommandTimeout { get; }
}