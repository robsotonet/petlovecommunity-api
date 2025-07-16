using PetLoveCommunity.Application.Configuration;

namespace PetLoveCommunity.Infrastructure.Data;

/// <summary>
/// Utility class for building PostgreSQL connection strings from configuration settings.
/// </summary>
/// <remarks>
/// This class handles only connection-level parameters. Error detail configuration
/// (such as sensitive data logging and detailed errors) is managed at the Entity Framework
/// level in the DependencyInjection configuration using EnableSensitiveDataLogging()
/// and EnableDetailedErrors() options.
/// </remarks>
public static class ConnectionStringBuilder
{
    /// <summary>
    /// Builds a PostgreSQL connection string from database settings and credentials.
    /// </summary>
    /// <param name="settings">The database configuration settings containing connection parameters.</param>
    /// <param name="credentials">The database credentials containing authentication information.</param>
    /// <returns>A properly formatted PostgreSQL connection string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when settings or credentials are null.</exception>
    /// <remarks>
    /// Error detail configuration from DatabaseSettings.IncludeErrorDetail is not applied here
    /// as it's handled at the Entity Framework Core level for proper separation of concerns.
    /// </remarks>
    public static string BuildConnectionString(DatabaseSettings settings, DatabaseCredentials credentials)
    {
        if (settings == null) throw new ArgumentNullException(nameof(settings));
        if (credentials == null) throw new ArgumentNullException(nameof(credentials));

        var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = settings.Host,
            Port = settings.Port,
            Database = credentials.DatabaseName,
            Username = credentials.Username,
            Password = credentials.Password,
            Timeout = settings.Timeout,
            MaxPoolSize = settings.MaxPoolSize,
            MinPoolSize = settings.MinPoolSize,
            CommandTimeout = settings.CommandTimeout
            // Note: Error detail configuration is handled at the EF Core level in DependencyInjection.cs
            // using EnableSensitiveDataLogging() and EnableDetailedErrors() options
        };

        // Handle SSL mode configuration
        if (Enum.TryParse<Npgsql.SslMode>(settings.SslMode, out var sslMode))
        {
            connectionStringBuilder.SslMode = sslMode;
        }

        return connectionStringBuilder.ToString();
    }
}