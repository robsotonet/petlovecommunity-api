using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using PetLoveCommunity.Application.Configuration;
using PetLoveCommunity.API.Services;

namespace PetLoveCommunity.Tests.Unit.API.Services;

public class DatabaseConnectionServiceTests
{
    private readonly Mock<IOptions<DatabaseSettings>> _mockSettingsOptions;
    private readonly Mock<IOptions<DatabaseCredentials>> _mockCredentialsOptions;
    private readonly Mock<IConfigurationValidator> _mockValidator;
    private readonly DatabaseSettings _defaultSettings;
    private readonly DatabaseCredentials _defaultCredentials;

    public DatabaseConnectionServiceTests()
    {
        _mockSettingsOptions = new Mock<IOptions<DatabaseSettings>>();
        _mockCredentialsOptions = new Mock<IOptions<DatabaseCredentials>>();
        _mockValidator = new Mock<IConfigurationValidator>();

        _defaultSettings = new DatabaseSettings
        {
            Host = "localhost",
            Port = 5432,
            Timeout = 30,
            MaxPoolSize = 20,
            MinPoolSize = 5,
            SslMode = "Prefer",
            IncludeErrorDetail = true,
            CommandTimeout = 30
        };

        _defaultCredentials = new DatabaseCredentials
        {
            Username = "testuser",
            Password = "testpassword123",
            DatabaseName = "testdatabase"
        };

        _mockSettingsOptions.Setup(x => x.Value).Returns(_defaultSettings);
        _mockCredentialsOptions.Setup(x => x.Value).Returns(_defaultCredentials);
    }

    [Fact]
    public void Constructor_ShouldCallValidationOnBothConfigurations()
    {
        // Act
        var service = new DatabaseConnectionService(
            _mockSettingsOptions.Object,
            _mockCredentialsOptions.Object,
            _mockValidator.Object);

        // Assert
        _mockValidator.Verify(x => x.ValidateAndThrow(_defaultSettings, "Database Settings"), Times.Once);
        _mockValidator.Verify(x => x.ValidateAndThrow(_defaultCredentials, "Database Credentials"), Times.Once);
    }

    [Fact]
    public void Constructor_WhenValidationFails_ShouldThrowException()
    {
        // Arrange
        _mockValidator.Setup(x => x.ValidateAndThrow(It.IsAny<DatabaseSettings>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException("Validation failed"));

        // Act & Assert
        var action = () => new DatabaseConnectionService(
            _mockSettingsOptions.Object,
            _mockCredentialsOptions.Object,
            _mockValidator.Object);

        action.Should().Throw<InvalidOperationException>().WithMessage("Validation failed");
    }

    [Fact]
    public void GetConnectionString_ShouldReturnCorrectlyFormattedConnectionString()
    {
        // Arrange
        var service = new DatabaseConnectionService(
            _mockSettingsOptions.Object,
            _mockCredentialsOptions.Object,
            _mockValidator.Object);

        // Act
        var connectionString = service.GetConnectionString();

        // Assert
        connectionString.Should().Contain("Host=localhost");
        connectionString.Should().Contain("Port=5432");
        connectionString.Should().Contain("Database=testdatabase");
        connectionString.Should().Contain("Username=testuser");
        connectionString.Should().Contain("Password=testpassword123");
        connectionString.Should().Contain("Timeout=30");
        connectionString.Should().Contain("Maximum Pool Size=20");
        connectionString.Should().Contain("Minimum Pool Size=5");
        connectionString.Should().Contain("SSL Mode=Prefer");
        connectionString.Should().Contain("Include Error Detail=True");
        connectionString.Should().Contain("Command Timeout=30");
    }

    [Fact]
    public void GetConnectionString_WithCustomDatabaseName_ShouldUseProvidedDatabaseName()
    {
        // Arrange
        var service = new DatabaseConnectionService(
            _mockSettingsOptions.Object,
            _mockCredentialsOptions.Object,
            _mockValidator.Object);
        var customDatabaseName = "custom_database";

        // Act
        var connectionString = service.GetConnectionString(customDatabaseName);

        // Assert
        connectionString.Should().Contain($"Database={customDatabaseName}");
        connectionString.Should().NotContain("Database=testdatabase");
    }

    [Fact]
    public void GetConnectionString_ShouldIncludeAllRequiredConnectionParameters()
    {
        // Arrange
        var customSettings = new DatabaseSettings
        {
            Host = "prod-server.example.com",
            Port = 5433,
            Timeout = 60,
            MaxPoolSize = 50,
            MinPoolSize = 10,
            SslMode = "Require",
            IncludeErrorDetail = false,
            CommandTimeout = 45
        };

        var customCredentials = new DatabaseCredentials
        {
            Username = "prod_user",
            Password = "complex!Password123",
            DatabaseName = "production_db"
        };

        _mockSettingsOptions.Setup(x => x.Value).Returns(customSettings);
        _mockCredentialsOptions.Setup(x => x.Value).Returns(customCredentials);

        var service = new DatabaseConnectionService(
            _mockSettingsOptions.Object,
            _mockCredentialsOptions.Object,
            _mockValidator.Object);

        // Act
        var connectionString = service.GetConnectionString();

        // Assert
        connectionString.Should().Contain("Host=prod-server.example.com");
        connectionString.Should().Contain("Port=5433");
        connectionString.Should().Contain("Database=production_db");
        connectionString.Should().Contain("Username=prod_user");
        connectionString.Should().Contain("Password=complex!Password123");
        connectionString.Should().Contain("Timeout=60");
        connectionString.Should().Contain("Maximum Pool Size=50");
        connectionString.Should().Contain("Minimum Pool Size=10");
        connectionString.Should().Contain("SSL Mode=Require");
        connectionString.Should().Contain("Include Error Detail=False");
        connectionString.Should().Contain("Command Timeout=45");
    }

    [Fact]
    public void GetConnectionString_ShouldHandleSpecialCharactersInPassword()
    {
        // Arrange
        var credentialsWithSpecialChars = new DatabaseCredentials
        {
            Username = "testuser",
            Password = "P@ssw0rd!#$%^&*()",
            DatabaseName = "testdb"
        };

        _mockCredentialsOptions.Setup(x => x.Value).Returns(credentialsWithSpecialChars);

        var service = new DatabaseConnectionService(
            _mockSettingsOptions.Object,
            _mockCredentialsOptions.Object,
            _mockValidator.Object);

        // Act
        var connectionString = service.GetConnectionString();

        // Assert
        connectionString.Should().Contain("Password=P@ssw0rd!#$%^&*();");
    }

    [Fact]
    public void GetConnectionString_ShouldEndWithSemicolon()
    {
        // Arrange
        var service = new DatabaseConnectionService(
            _mockSettingsOptions.Object,
            _mockCredentialsOptions.Object,
            _mockValidator.Object);

        // Act
        var connectionString = service.GetConnectionString();

        // Assert
        connectionString.Should().EndWith(";");
    }

    [Fact]
    public void GetConnectionString_DefaultOverload_ShouldUseCredentialsDatabaseName()
    {
        // Arrange
        var service = new DatabaseConnectionService(
            _mockSettingsOptions.Object,
            _mockCredentialsOptions.Object,
            _mockValidator.Object);

        // Act
        var connectionString = service.GetConnectionString();

        // Assert
        connectionString.Should().Contain("Database=testdatabase;");
    }

    [Theory]
    [InlineData("test_db")]
    [InlineData("production")]
    [InlineData("development_database")]
    [InlineData("db-with-hyphens")]
    public void GetConnectionString_WithDifferentDatabaseNames_ShouldFormatCorrectly(string databaseName)
    {
        // Arrange
        var service = new DatabaseConnectionService(
            _mockSettingsOptions.Object,
            _mockCredentialsOptions.Object,
            _mockValidator.Object);

        // Act
        var connectionString = service.GetConnectionString(databaseName);

        // Assert
        connectionString.Should().Contain($"Database={databaseName};");
    }

    [Fact]
    public void ImplementsInterface_ShouldImplementIDatabaseConnectionService()
    {
        // Arrange & Act
        var service = new DatabaseConnectionService(
            _mockSettingsOptions.Object,
            _mockCredentialsOptions.Object,
            _mockValidator.Object);

        // Assert
        service.Should().BeAssignableTo<IDatabaseConnectionService>();
    }

    [Fact]
    public void GetConnectionString_ShouldBeReproducible()
    {
        // Arrange
        var service = new DatabaseConnectionService(
            _mockSettingsOptions.Object,
            _mockCredentialsOptions.Object,
            _mockValidator.Object);

        // Act
        var connectionString1 = service.GetConnectionString();
        var connectionString2 = service.GetConnectionString();

        // Assert
        connectionString1.Should().Be(connectionString2);
    }

    [Fact]
    public void GetConnectionString_WithBooleanValues_ShouldFormatCorrectly()
    {
        // Arrange
        var settingsWithFalseBoolean = new DatabaseSettings
        {
            Host = "localhost",
            Port = 5432,
            Timeout = 30,
            MaxPoolSize = 20,
            MinPoolSize = 5,
            SslMode = "Prefer",
            IncludeErrorDetail = false, // Test false boolean
            CommandTimeout = 30
        };

        _mockSettingsOptions.Setup(x => x.Value).Returns(settingsWithFalseBoolean);

        var service = new DatabaseConnectionService(
            _mockSettingsOptions.Object,
            _mockCredentialsOptions.Object,
            _mockValidator.Object);

        // Act
        var connectionString = service.GetConnectionString();

        // Assert
        connectionString.Should().Contain("Include Error Detail=False;");
    }

    [Fact]
    public void GetConnectionString_ShouldNotContainExtraSpaces()
    {
        // Arrange
        var service = new DatabaseConnectionService(
            _mockSettingsOptions.Object,
            _mockCredentialsOptions.Object,
            _mockValidator.Object);

        // Act
        var connectionString = service.GetConnectionString();

        // Assert
        connectionString.Should().NotContain("  "); // No double spaces
        connectionString.Should().NotStartWith(" ");
        connectionString.Should().NotEndWith(" ;");
    }

    [Fact]
    public void Constructor_ShouldStoreConfigurationValues()
    {
        // Arrange
        var service = new DatabaseConnectionService(
            _mockSettingsOptions.Object,
            _mockCredentialsOptions.Object,
            _mockValidator.Object);

        // Act & Assert - Test that the service uses the injected configurations
        // by verifying the connection string contains expected values
        var connectionString = service.GetConnectionString();
        
        connectionString.Should().Contain(_defaultSettings.Host);
        connectionString.Should().Contain(_defaultSettings.Port.ToString());
        connectionString.Should().Contain(_defaultCredentials.Username);
        connectionString.Should().Contain(_defaultCredentials.DatabaseName);
    }
}