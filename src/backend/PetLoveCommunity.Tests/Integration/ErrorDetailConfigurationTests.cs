using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetLoveCommunity.Application.Configuration;
using PetLoveCommunity.Infrastructure;
using PetLoveCommunity.Infrastructure.Data;

namespace PetLoveCommunity.Tests.Integration;

public class ErrorDetailConfigurationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public ErrorDetailConfigurationTests()
    {
        // Create test configuration
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:Host"] = "localhost",
            ["Database:Port"] = "5432",
            ["Database:Timeout"] = "30",
            ["Database:MaxPoolSize"] = "20",
            ["Database:MinPoolSize"] = "5",
            ["Database:SslMode"] = "Prefer",
            ["Database:CommandTimeout"] = "30",
            ["Database:IncludeErrorDetail"] = "true",
            ["DatabaseCredentials:DatabaseName"] = "test_db",
            ["DatabaseCredentials:Username"] = "test_user",
            ["DatabaseCredentials:Password"] = "test_password"
        });
        _configuration = configBuilder.Build();

        // Create service collection and add infrastructure
        var services = new ServiceCollection();
        services.AddLogging();
        
        // This should not throw an exception even though we're testing the connection string building
        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    #region Configuration Tests

    [Fact]
    public void DatabaseSettings_IncludeErrorDetail_ShouldBeConfigurable()
    {
        // Act
        var databaseSettings = _configuration.GetSection("Database").Get<DatabaseSettings>();

        // Assert
        databaseSettings.Should().NotBeNull();
        databaseSettings!.IncludeErrorDetail.Should().BeTrue();
    }

    [Fact]
    public void DatabaseSettings_IncludeErrorDetail_ShouldDefaultToTrue()
    {
        // Arrange
        var settings = new DatabaseSettings();

        // Act & Assert
        settings.IncludeErrorDetail.Should().BeTrue();
    }

    #endregion

    #region Connection String Building Tests

    [Fact]
    public void ConnectionStringBuilder_ShouldNotIncludeErrorDetailInConnectionString()
    {
        // Arrange
        var databaseSettings = _configuration.GetSection("Database").Get<DatabaseSettings>()!;
        var databaseCredentials = _configuration.GetSection("DatabaseCredentials").Get<DatabaseCredentials>()!;

        // Act
        var connectionString = ConnectionStringBuilder.BuildConnectionString(databaseSettings, databaseCredentials);

        // Assert
        connectionString.Should().NotBeNullOrEmpty();
        connectionString.Should().NotContain("IncludeErrorDetail");
        connectionString.Should().NotContain("Include Error Detail");
        
        // Verify the connection string is valid
        var act = () => new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConnectionStringBuilder_WithDifferentErrorDetailSettings_ShouldProduceSameConnectionString(bool includeErrorDetail)
    {
        // Arrange
        var settings1 = new DatabaseSettings
        {
            Host = "localhost",
            Port = 5432,
            Timeout = 30,
            MaxPoolSize = 20,
            MinPoolSize = 5,
            SslMode = "Prefer",
            CommandTimeout = 30,
            IncludeErrorDetail = true
        };

        var settings2 = new DatabaseSettings
        {
            Host = "localhost",
            Port = 5432,
            Timeout = 30,
            MaxPoolSize = 20,
            MinPoolSize = 5,
            SslMode = "Prefer",
            CommandTimeout = 30,
            IncludeErrorDetail = includeErrorDetail
        };

        var credentials = new DatabaseCredentials
        {
            DatabaseName = "testdb",
            Username = "testuser",
            Password = "testpassword"
        };

        // Act
        var connectionString1 = ConnectionStringBuilder.BuildConnectionString(settings1, credentials);
        var connectionString2 = ConnectionStringBuilder.BuildConnectionString(settings2, credentials);

        // Assert
        connectionString1.Should().Be(connectionString2);
    }

    #endregion

    #region Infrastructure Configuration Tests

    [Fact]
    public void InfrastructureConfiguration_WithIncludeErrorDetailTrue_ShouldConfigureDbContextOptions()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:Host"] = "localhost",
            ["Database:Port"] = "5432",
            ["Database:Timeout"] = "30",
            ["Database:MaxPoolSize"] = "20",
            ["Database:MinPoolSize"] = "5",
            ["Database:SslMode"] = "Prefer",
            ["Database:CommandTimeout"] = "30",
            ["Database:IncludeErrorDetail"] = "true",
            ["DatabaseCredentials:DatabaseName"] = "test_db",
            ["DatabaseCredentials:Username"] = "test_user",
            ["DatabaseCredentials:Password"] = "test_password"
        });
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var act = () => services.AddInfrastructure(configuration);

        // Assert
        // This should not throw an exception, even though the database doesn't exist
        // We're testing the configuration setup, not the actual database connection
        act.Should().NotThrow();
    }

    [Fact]
    public void InfrastructureConfiguration_WithIncludeErrorDetailFalse_ShouldConfigureDbContextOptions()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:Host"] = "localhost",
            ["Database:Port"] = "5432",
            ["Database:Timeout"] = "30",
            ["Database:MaxPoolSize"] = "20",
            ["Database:MinPoolSize"] = "5",
            ["Database:SslMode"] = "Prefer",
            ["Database:CommandTimeout"] = "30",
            ["Database:IncludeErrorDetail"] = "false",
            ["DatabaseCredentials:DatabaseName"] = "test_db",
            ["DatabaseCredentials:Username"] = "test_user",
            ["DatabaseCredentials:Password"] = "test_password"
        });
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var act = () => services.AddInfrastructure(configuration);

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region DbContext Configuration Tests

    [Fact]
    public void DbContextOptionsConfiguration_ShouldHandleErrorDetailSettings()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:Host"] = "localhost",
            ["Database:Port"] = "5432",
            ["Database:Timeout"] = "30",
            ["Database:MaxPoolSize"] = "20",
            ["Database:MinPoolSize"] = "5",
            ["Database:SslMode"] = "Prefer",
            ["Database:CommandTimeout"] = "30",
            ["Database:IncludeErrorDetail"] = "true",
            ["DatabaseCredentials:DatabaseName"] = "test_db_memory",
            ["DatabaseCredentials:Username"] = "test_user",
            ["DatabaseCredentials:Password"] = "test_password"
        });
        var configuration = configBuilder.Build();

        // Override with in-memory database for testing
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase("TestDatabase");
            
            // Simulate the same error detail configuration logic from DependencyInjection
            if (configuration.GetValue<bool>("Database:IncludeErrorDetail", false))
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        dbContext.Should().NotBeNull();
        
        // Verify that the context can be used (this indirectly tests the configuration)
        var act = () => dbContext.Database.EnsureCreated();
        act.Should().NotThrow();
    }

    #endregion

    #region End-to-End Configuration Test

    [Fact]
    public void EndToEnd_ErrorDetailConfiguration_ShouldWorkCorrectly()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:Host"] = "localhost",
            ["Database:Port"] = "5432",
            ["Database:Timeout"] = "30",
            ["Database:MaxPoolSize"] = "20",
            ["Database:MinPoolSize"] = "5",
            ["Database:SslMode"] = "Prefer",
            ["Database:CommandTimeout"] = "30",
            ["Database:IncludeErrorDetail"] = "true",
            ["DatabaseCredentials:DatabaseName"] = "test_db",
            ["DatabaseCredentials:Username"] = "test_user",
            ["DatabaseCredentials:Password"] = "test_password"
        });
        var configuration = configBuilder.Build();

        // Act
        // 1. Parse configuration
        var databaseSettings = configuration.GetSection("Database").Get<DatabaseSettings>();
        var databaseCredentials = configuration.GetSection("DatabaseCredentials").Get<DatabaseCredentials>();

        // 2. Build connection string (should not include error detail)
        var connectionString = ConnectionStringBuilder.BuildConnectionString(databaseSettings!, databaseCredentials!);

        // 3. Verify configuration can be used for DI setup
        var services = new ServiceCollection();
        services.AddLogging();
        var setupAction = () => services.AddInfrastructure(configuration);

        // Assert
        databaseSettings!.IncludeErrorDetail.Should().BeTrue();
        connectionString.Should().NotContain("IncludeErrorDetail");
        setupAction.Should().NotThrow();

        // Verify the connection string is valid for Npgsql
        var npgsqlBuilder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        npgsqlBuilder.Host.Should().Be("localhost");
        npgsqlBuilder.Database.Should().Be("test_db");
    }

    #endregion
}