using PetLoveCommunity.Application.Configuration;

namespace PetLoveCommunity.Infrastructure.Data;

public static class ConnectionStringBuilder
{
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
            CommandTimeout = settings.CommandTimeout,
            IncludeErrorDetail = settings.IncludeErrorDetail
        };

        // Handle SSL mode configuration
        if (Enum.TryParse<Npgsql.SslMode>(settings.SslMode, out var sslMode))
        {
            connectionStringBuilder.SslMode = sslMode;
        }

        return connectionStringBuilder.ToString();
    }
}