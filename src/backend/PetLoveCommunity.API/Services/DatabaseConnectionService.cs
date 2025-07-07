using Microsoft.Extensions.Options;
using PetLoveCommunity.API.Configuration;
using System.Text;

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
        var connectionString = new StringBuilder();
        
        connectionString.Append($"Host={_settings.Host};");
        connectionString.Append($"Port={_settings.Port};");
        connectionString.Append($"Database={databaseName};");
        connectionString.Append($"Username={_credentials.Username};");
        connectionString.Append($"Password={_credentials.Password};");
        connectionString.Append($"Timeout={_settings.Timeout};");
        connectionString.Append($"Maximum Pool Size={_settings.MaxPoolSize};");
        connectionString.Append($"Minimum Pool Size={_settings.MinPoolSize};");
        connectionString.Append($"SSL Mode={_settings.SslMode};");
        connectionString.Append($"Include Error Detail={_settings.IncludeErrorDetail};");
        connectionString.Append($"Command Timeout={_settings.CommandTimeout};");

        return connectionString.ToString();
    }

    private void ValidateConfiguration()
    {
        _validator.ValidateAndThrow(_settings, "Database Settings");
        _validator.ValidateAndThrow(_credentials, "Database Credentials");
    }
}