using FluentAssertions;
using PetLoveCommunity.Application.Configuration;
using PetLoveCommunity.Infrastructure.Data;

namespace PetLoveCommunity.Tests.Unit.Infrastructure.Data;

public class ConnectionStringBuilderTests
{
    #region Helper Methods

    private static DatabaseSettings CreateValidDatabaseSettings()
    {
        return new DatabaseSettings
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
    }

    private static DatabaseCredentials CreateValidDatabaseCredentials()
    {
        return new DatabaseCredentials
        {
            DatabaseName = "testdb",
            Username = "testuser",
            Password = "testpassword"
        };
    }

    #endregion

    #region Constructor Validation Tests

    [Fact]
    public void BuildConnectionString_WithNullSettings_ShouldThrowArgumentNullException()
    {
        // Arrange
        var credentials = CreateValidDatabaseCredentials();

        // Act & Assert
        var act = () => ConnectionStringBuilder.BuildConnectionString(null!, credentials);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("settings");
    }

    [Fact]
    public void BuildConnectionString_WithNullCredentials_ShouldThrowArgumentNullException()
    {
        // Arrange
        var settings = CreateValidDatabaseSettings();

        // Act & Assert
        var act = () => ConnectionStringBuilder.BuildConnectionString(settings, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("credentials");
    }

    [Fact]
    public void BuildConnectionString_WithBothParametersNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => ConnectionStringBuilder.BuildConnectionString(null!, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("settings");
    }

    #endregion

    #region Valid Connection String Generation Tests

    [Fact]
    public void BuildConnectionString_WithValidParameters_ShouldGenerateValidConnectionString()
    {
        // Arrange
        var settings = CreateValidDatabaseSettings();
        var credentials = CreateValidDatabaseCredentials();

        // Act
        var connectionString = ConnectionStringBuilder.BuildConnectionString(settings, credentials);

        // Assert
        connectionString.Should().NotBeNullOrEmpty();
        connectionString.Should().Contain($"Host={settings.Host}");
        connectionString.Should().Contain($"Port={settings.Port}");
        connectionString.Should().Contain($"Database={credentials.DatabaseName}");
        connectionString.Should().Contain($"Username={credentials.Username}");
        connectionString.Should().Contain($"Password={credentials.Password}");
        connectionString.Should().Contain($"Timeout={settings.Timeout}");
        connectionString.Should().Contain($"Maximum Pool Size={settings.MaxPoolSize}");
        connectionString.Should().Contain($"Minimum Pool Size={settings.MinPoolSize}");
        connectionString.Should().Contain($"Command Timeout={settings.CommandTimeout}");
        connectionString.Should().Contain($"SSL Mode={settings.SslMode}");
    }

    [Fact]
    public void BuildConnectionString_ShouldNotContainIncludeErrorDetail()
    {
        // Arrange
        var settings = new DatabaseSettings
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
        var credentials = CreateValidDatabaseCredentials();

        // Act
        var connectionString = ConnectionStringBuilder.BuildConnectionString(settings, credentials);

        // Assert
        connectionString.Should().NotContain("IncludeErrorDetail");
        connectionString.Should().NotContain("Include Error Detail");
    }

    [Fact]
    public void BuildConnectionString_WithSpecialCharactersInCredentials_ShouldEscapeProperly()
    {
        // Arrange
        var settings = CreateValidDatabaseSettings();
        var credentials = new DatabaseCredentials
        {
            DatabaseName = "test-db_name",
            Username = "test.user@domain",
            Password = "p@ssw0rd!@#$%"
        };

        // Act
        var connectionString = ConnectionStringBuilder.BuildConnectionString(settings, credentials);

        // Assert
        connectionString.Should().NotBeNullOrEmpty();
        connectionString.Should().Contain($"Database={credentials.DatabaseName}");
        connectionString.Should().Contain($"Username={credentials.Username}");
        connectionString.Should().Contain($"Password={credentials.Password}");
    }

    #endregion

    #region SSL Mode Configuration Tests

    [Theory]
    [InlineData("Disable")]
    [InlineData("Allow")]
    [InlineData("Prefer")]
    [InlineData("Require")]
    [InlineData("VerifyCA")]
    [InlineData("VerifyFull")]
    public void BuildConnectionString_WithValidSslMode_ShouldIncludeSslMode(string sslMode)
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            Host = "localhost",
            Port = 5432,
            Timeout = 30,
            MaxPoolSize = 20,
            MinPoolSize = 5,
            SslMode = sslMode,
            IncludeErrorDetail = true,
            CommandTimeout = 30
        };
        var credentials = CreateValidDatabaseCredentials();

        // Act
        var connectionString = ConnectionStringBuilder.BuildConnectionString(settings, credentials);

        // Assert
        connectionString.Should().Contain($"SSL Mode={sslMode}");
    }

    [Fact]
    public void BuildConnectionString_WithInvalidSslMode_ShouldNotIncludeSslMode()
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            Host = "localhost",
            Port = 5432,
            Timeout = 30,
            MaxPoolSize = 20,
            MinPoolSize = 5,
            SslMode = "InvalidMode",
            IncludeErrorDetail = true,
            CommandTimeout = 30
        };
        var credentials = CreateValidDatabaseCredentials();

        // Act
        var connectionString = ConnectionStringBuilder.BuildConnectionString(settings, credentials);

        // Assert
        connectionString.Should().NotContain("SSL Mode=InvalidMode");
        // The connection string should still be generated without the invalid SSL mode
        connectionString.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void BuildConnectionString_WithEmptyOrNullSslMode_ShouldNotIncludeSslMode(string? sslMode)
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            Host = "localhost",
            Port = 5432,
            Timeout = 30,
            MaxPoolSize = 20,
            MinPoolSize = 5,
            SslMode = sslMode!,
            IncludeErrorDetail = true,
            CommandTimeout = 30
        };
        var credentials = CreateValidDatabaseCredentials();

        // Act
        var connectionString = ConnectionStringBuilder.BuildConnectionString(settings, credentials);

        // Assert
        connectionString.Should().NotContain("SSL Mode=");
        connectionString.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Connection Parameters Tests

    [Theory]
    [InlineData(1433)]
    [InlineData(5432)]
    [InlineData(3306)]
    [InlineData(65535)]
    public void BuildConnectionString_WithDifferentPorts_ShouldIncludeCorrectPort(int port)
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            Host = "localhost",
            Port = port,
            Timeout = 30,
            MaxPoolSize = 20,
            MinPoolSize = 5,
            SslMode = "Prefer",
            IncludeErrorDetail = true,
            CommandTimeout = 30
        };
        var credentials = CreateValidDatabaseCredentials();

        // Act
        var connectionString = ConnectionStringBuilder.BuildConnectionString(settings, credentials);

        // Assert
        connectionString.Should().Contain($"Port={port}");
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 20)]
    [InlineData(10, 100)]
    public void BuildConnectionString_WithDifferentPoolSizes_ShouldIncludeCorrectSizes(int minPoolSize, int maxPoolSize)
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            Host = "localhost",
            Port = 5432,
            Timeout = 30,
            MaxPoolSize = maxPoolSize,
            MinPoolSize = minPoolSize,
            SslMode = "Prefer",
            IncludeErrorDetail = true,
            CommandTimeout = 30
        };
        var credentials = CreateValidDatabaseCredentials();

        // Act
        var connectionString = ConnectionStringBuilder.BuildConnectionString(settings, credentials);

        // Assert
        connectionString.Should().Contain($"Minimum Pool Size={minPoolSize}");
        connectionString.Should().Contain($"Maximum Pool Size={maxPoolSize}");
    }

    [Theory]
    [InlineData(10)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(300)]
    public void BuildConnectionString_WithDifferentTimeouts_ShouldIncludeCorrectTimeouts(int timeout)
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            Host = "localhost",
            Port = 5432,
            Timeout = timeout,
            MaxPoolSize = 20,
            MinPoolSize = 5,
            SslMode = "Prefer",
            IncludeErrorDetail = true,
            CommandTimeout = timeout
        };
        var credentials = CreateValidDatabaseCredentials();

        // Act
        var connectionString = ConnectionStringBuilder.BuildConnectionString(settings, credentials);

        // Assert
        connectionString.Should().Contain($"Timeout={timeout}");
        connectionString.Should().Contain($"Command Timeout={timeout}");
    }

    #endregion

    #region Host Configuration Tests

    [Theory]
    [InlineData("localhost")]
    [InlineData("127.0.0.1")]
    [InlineData("db.example.com")]
    [InlineData("192.168.1.100")]
    [InlineData("my-postgres-instance.amazonaws.com")]
    public void BuildConnectionString_WithDifferentHosts_ShouldIncludeCorrectHost(string host)
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            Host = host,
            Port = 5432,
            Timeout = 30,
            MaxPoolSize = 20,
            MinPoolSize = 5,
            SslMode = "Prefer",
            IncludeErrorDetail = true,
            CommandTimeout = 30
        };
        var credentials = CreateValidDatabaseCredentials();

        // Act
        var connectionString = ConnectionStringBuilder.BuildConnectionString(settings, credentials);

        // Assert
        connectionString.Should().Contain($"Host={host}");
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void BuildConnectionString_WithMinimumValidValues_ShouldGenerateConnectionString()
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            Host = "localhost",
            Port = 1,
            Timeout = 1,
            MaxPoolSize = 1,
            MinPoolSize = 0,
            SslMode = "Disable",
            IncludeErrorDetail = false,
            CommandTimeout = 1
        };
        var credentials = new DatabaseCredentials
        {
            DatabaseName = "db",
            Username = "u",
            Password = "p"
        };

        // Act
        var connectionString = ConnectionStringBuilder.BuildConnectionString(settings, credentials);

        // Assert
        connectionString.Should().NotBeNullOrEmpty();
        connectionString.Should().Contain("Host=localhost");
        connectionString.Should().Contain("Port=1");
        connectionString.Should().Contain("Database=db");
        connectionString.Should().Contain("Username=u");
        connectionString.Should().Contain("Password=p");
    }

    [Fact]
    public void BuildConnectionString_WithMaximumValidValues_ShouldGenerateConnectionString()
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            Host = new string('a', 50),
            Port = 65535,
            Timeout = 300,
            MaxPoolSize = 1000,
            MinPoolSize = 100,
            SslMode = "VerifyFull",
            IncludeErrorDetail = true,
            CommandTimeout = 3600
        };
        var credentials = new DatabaseCredentials
        {
            DatabaseName = new string('d', 50),
            Username = new string('u', 50),
            Password = new string('p', 50)
        };

        // Act
        var connectionString = ConnectionStringBuilder.BuildConnectionString(settings, credentials);

        // Assert
        connectionString.Should().NotBeNullOrEmpty();
        connectionString.Should().Contain($"Host={settings.Host}");
        connectionString.Should().Contain($"Port={settings.Port}");
        connectionString.Should().Contain($"Database={credentials.DatabaseName}");
    }

    [Fact]
    public void BuildConnectionString_ShouldReturnConsistentResults()
    {
        // Arrange
        var settings = CreateValidDatabaseSettings();
        var credentials = CreateValidDatabaseCredentials();

        // Act
        var connectionString1 = ConnectionStringBuilder.BuildConnectionString(settings, credentials);
        var connectionString2 = ConnectionStringBuilder.BuildConnectionString(settings, credentials);

        // Assert
        connectionString1.Should().Be(connectionString2);
    }

    [Fact]
    public void BuildConnectionString_WithIdenticalSettings_ShouldProduceSameConnectionString()
    {
        // Arrange
        var settings1 = CreateValidDatabaseSettings();
        var settings2 = CreateValidDatabaseSettings();
        var credentials1 = CreateValidDatabaseCredentials();
        var credentials2 = CreateValidDatabaseCredentials();

        // Act
        var connectionString1 = ConnectionStringBuilder.BuildConnectionString(settings1, credentials1);
        var connectionString2 = ConnectionStringBuilder.BuildConnectionString(settings2, credentials2);

        // Assert
        connectionString1.Should().Be(connectionString2);
    }

    #endregion

    #region Connection String Format Tests

    [Fact]
    public void BuildConnectionString_ShouldReturnWellFormedConnectionString()
    {
        // Arrange
        var settings = CreateValidDatabaseSettings();
        var credentials = CreateValidDatabaseCredentials();

        // Act
        var connectionString = ConnectionStringBuilder.BuildConnectionString(settings, credentials);

        // Assert
        connectionString.Should().NotBeNullOrEmpty();
        
        // Should not start or end with semicolon
        connectionString.Should().NotStartWith(";");
        connectionString.Should().NotEndWith(";");
        
        // Should contain key-value pairs separated by semicolons
        connectionString.Should().MatchRegex(@"^[^;]+(;[^;]+)*$");
        
        // Should contain equals signs for key-value pairs
        connectionString.Split(';').Should().OnlyContain(part => part.Contains('='));
    }

    [Fact]
    public void BuildConnectionString_ShouldBeValidNpgsqlConnectionString()
    {
        // Arrange
        var settings = CreateValidDatabaseSettings();
        var credentials = CreateValidDatabaseCredentials();

        // Act
        var connectionString = ConnectionStringBuilder.BuildConnectionString(settings, credentials);

        // Assert
        // Verify it can be parsed by NpgsqlConnectionStringBuilder
        var act = () => new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        act.Should().NotThrow();
        
        var parsedBuilder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        parsedBuilder.Host.Should().Be(settings.Host);
        parsedBuilder.Port.Should().Be(settings.Port);
        parsedBuilder.Database.Should().Be(credentials.DatabaseName);
        parsedBuilder.Username.Should().Be(credentials.Username);
        parsedBuilder.Password.Should().Be(credentials.Password);
    }

    #endregion
}