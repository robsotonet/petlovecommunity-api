using Microsoft.Extensions.Options;
using Npgsql;
using PetLoveCommunity.API.Configuration;

namespace PetLoveCommunity.API.Services;

/// <summary>
/// Service for generating database connection strings from configuration.
/// </summary>
public class DatabaseConnectionService : IDatabaseConnectionService
{
    private readonly DatabaseSettings _settings;
    private readonly DatabaseCredentials _credentials;
    private readonly IConfigurationValidator _validator;

    public DatabaseConnectionService(
        IOptions<DatabaseSettings> settings,
        IOptions<DatabaseCredentials> credentials,
        IConfigurationValidator validator)
    {
        _settings = settings.Value;
        _credentials = credentials.Value;
        _validator = validator;
        ValidateConfiguration();
    }

    public string GetConnectionString()
    {
        return GetConnectionString(_credentials.DatabaseName);
    }

    public string GetConnectionString(string databaseName)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = _settings.Host,
            Port = _settings.Port,
            Database = databaseName,
            Username = _credentials.Username,
            Password = _credentials.Password,
            Timeout = _settings.Timeout,
            MaxPoolSize = _settings.MaxPoolSize,
            MinPoolSize = _settings.MinPoolSize,
            SslMode = Enum.Parse<SslMode>(_settings.SslMode),
            IncludeErrorDetail = _settings.IncludeErrorDetail,
            CommandTimeout = _settings.CommandTimeout
        };

        var connectionString = builder.ConnectionString;
        
        // Ensure trailing semicolon for consistency with previous behavior
        if (!connectionString.EndsWith(";"))
        {
            connectionString += ";";
        }
        
        return connectionString;
    }

    private void ValidateConfiguration()
    {
        _validator.ValidateAndThrow(_settings, "Database Settings");
        _validator.ValidateAndThrow(_credentials, "Database Credentials");
    }
}