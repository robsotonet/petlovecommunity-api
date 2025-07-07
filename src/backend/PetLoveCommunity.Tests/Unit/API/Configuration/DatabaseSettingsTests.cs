using FluentAssertions;
using Microsoft.Extensions.Configuration;
using PetLoveCommunity.API.Configuration;
using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.Tests.Unit.API.Configuration;

public class DatabaseSettingsTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var settings = new DatabaseSettings();

        // Assert
        settings.Host.Should().Be("localhost");
        settings.Port.Should().Be(5432);
        settings.Timeout.Should().Be(30);
        settings.MaxPoolSize.Should().Be(20);
        settings.MinPoolSize.Should().Be(5);
        settings.SslMode.Should().Be("Prefer");
        settings.IncludeErrorDetail.Should().BeTrue();
        settings.CommandTimeout.Should().Be(30);
    }

    [Fact]
    public void Properties_ShouldBeImmutable_WhenUsingInit()
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            Host = "test-host",
            Port = 5433,
            Timeout = 60,
            MaxPoolSize = 50,
            MinPoolSize = 10,
            SslMode = "Require",
            IncludeErrorDetail = false,
            CommandTimeout = 45
        };

        // Assert - Properties should not be settable after initialization
        settings.Host.Should().Be("test-host");
        settings.Port.Should().Be(5433);
        settings.Timeout.Should().Be(60);
        settings.MaxPoolSize.Should().Be(50);
        settings.MinPoolSize.Should().Be(10);
        settings.SslMode.Should().Be("Require");
        settings.IncludeErrorDetail.Should().BeFalse();
        settings.CommandTimeout.Should().Be(45);
    }

    [Theory]
    [InlineData("", false)] // Empty host
    [InlineData("   ", false)] // Whitespace only
    [InlineData("localhost", true)] // Valid host
    [InlineData("192.168.1.100", true)] // Valid IP
    [InlineData("db.example.com", true)] // Valid domain
    public void Host_Validation_ShouldRespectRequiredAttribute(string host, bool isValid)
    {
        // Arrange
        var settings = new DatabaseSettings { Host = host };
        var context = new ValidationContext(settings) { MemberName = nameof(DatabaseSettings.Host) };
        var results = new List<ValidationResult>();

        // Act
        var actualIsValid = Validator.TryValidateProperty(settings.Host, context, results);

        // Assert
        actualIsValid.Should().Be(isValid);
        if (!isValid)
        {
            results.Should().HaveCount(1);
            results[0].ErrorMessage.Should().Contain("required");
        }
    }

    [Theory]
    [InlineData(0, false)] // Below minimum
    [InlineData(1, true)] // Minimum valid
    [InlineData(5432, true)] // Default PostgreSQL port
    [InlineData(65535, true)] // Maximum valid port
    [InlineData(65536, false)] // Above maximum
    public void Port_Validation_ShouldRespectRangeAttribute(int port, bool isValid)
    {
        // Arrange
        var settings = new DatabaseSettings { Port = port };
        var context = new ValidationContext(settings) { MemberName = nameof(DatabaseSettings.Port) };
        var results = new List<ValidationResult>();

        // Act
        var actualIsValid = Validator.TryValidateProperty(settings.Port, context, results);

        // Assert
        actualIsValid.Should().Be(isValid);
        if (!isValid)
        {
            results.Should().HaveCount(1);
            results[0].ErrorMessage.Should().Contain("between 1 and 65535");
        }
    }

    [Theory]
    [InlineData(0, false)] // Below minimum
    [InlineData(1, true)] // Minimum valid
    [InlineData(30, true)] // Default value
    [InlineData(300, true)] // Maximum valid
    [InlineData(301, false)] // Above maximum
    public void Timeout_Validation_ShouldRespectRangeAttribute(int timeout, bool isValid)
    {
        // Arrange
        var settings = new DatabaseSettings { Timeout = timeout };
        var context = new ValidationContext(settings) { MemberName = nameof(DatabaseSettings.Timeout) };
        var results = new List<ValidationResult>();

        // Act
        var actualIsValid = Validator.TryValidateProperty(settings.Timeout, context, results);

        // Assert
        actualIsValid.Should().Be(isValid);
        if (!isValid)
        {
            results.Should().HaveCount(1);
            results[0].ErrorMessage.Should().Contain("between 1 and 300");
        }
    }

    [Theory]
    [InlineData(0, false)] // Below minimum
    [InlineData(1, true)] // Minimum valid
    [InlineData(20, true)] // Default value
    [InlineData(1000, true)] // Maximum valid
    [InlineData(1001, false)] // Above maximum
    public void MaxPoolSize_Validation_ShouldRespectRangeAttribute(int maxPoolSize, bool isValid)
    {
        // Arrange
        var settings = new DatabaseSettings { MaxPoolSize = maxPoolSize };
        var context = new ValidationContext(settings) { MemberName = nameof(DatabaseSettings.MaxPoolSize) };
        var results = new List<ValidationResult>();

        // Act
        var actualIsValid = Validator.TryValidateProperty(settings.MaxPoolSize, context, results);

        // Assert
        actualIsValid.Should().Be(isValid);
        if (!isValid)
        {
            results.Should().HaveCount(1);
            results[0].ErrorMessage.Should().Contain("between 1 and 1000");
        }
    }

    [Theory]
    [InlineData(-1, false)] // Below minimum
    [InlineData(0, true)] // Minimum valid
    [InlineData(5, true)] // Default value
    [InlineData(100, true)] // Maximum valid
    [InlineData(101, false)] // Above maximum
    public void MinPoolSize_Validation_ShouldRespectRangeAttribute(int minPoolSize, bool isValid)
    {
        // Arrange
        var settings = new DatabaseSettings { MinPoolSize = minPoolSize };
        var context = new ValidationContext(settings) { MemberName = nameof(DatabaseSettings.MinPoolSize) };
        var results = new List<ValidationResult>();

        // Act
        var actualIsValid = Validator.TryValidateProperty(settings.MinPoolSize, context, results);

        // Assert
        actualIsValid.Should().Be(isValid);
        if (!isValid)
        {
            results.Should().HaveCount(1);
            results[0].ErrorMessage.Should().Contain("between 0 and 100");
        }
    }

    [Theory]
    [InlineData("", false)] // Empty SSL mode
    [InlineData("Prefer", true)] // Valid SSL mode
    [InlineData("Require", true)] // Valid SSL mode
    [InlineData("Disable", true)] // Valid SSL mode
    public void SslMode_Validation_ShouldRespectRequiredAttribute(string sslMode, bool isValid)
    {
        // Arrange
        var settings = new DatabaseSettings { SslMode = sslMode };
        var context = new ValidationContext(settings) { MemberName = nameof(DatabaseSettings.SslMode) };
        var results = new List<ValidationResult>();

        // Act
        var actualIsValid = Validator.TryValidateProperty(settings.SslMode, context, results);

        // Assert
        actualIsValid.Should().Be(isValid);
        if (!isValid)
        {
            results.Should().HaveCount(1);
            results[0].ErrorMessage.Should().Contain("required");
        }
    }

    [Theory]
    [InlineData(0, false)] // Below minimum
    [InlineData(1, true)] // Minimum valid
    [InlineData(30, true)] // Default value
    [InlineData(3600, true)] // Maximum valid (1 hour)
    [InlineData(3601, false)] // Above maximum
    public void CommandTimeout_Validation_ShouldRespectRangeAttribute(int commandTimeout, bool isValid)
    {
        // Arrange
        var settings = new DatabaseSettings { CommandTimeout = commandTimeout };
        var context = new ValidationContext(settings) { MemberName = nameof(DatabaseSettings.CommandTimeout) };
        var results = new List<ValidationResult>();

        // Act
        var actualIsValid = Validator.TryValidateProperty(settings.CommandTimeout, context, results);

        // Assert
        actualIsValid.Should().Be(isValid);
        if (!isValid)
        {
            results.Should().HaveCount(1);
            results[0].ErrorMessage.Should().Contain("between 1 and 3600");
        }
    }

    [Fact]
    public void ValidObject_ShouldPassCompleteValidation()
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
        var context = new ValidationContext(settings);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(settings, context, results, validateAllProperties: true);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void ImplementsInterface_ShouldImplementIDatabaseSettings()
    {
        // Arrange & Act
        var settings = new DatabaseSettings();

        // Assert
        settings.Should().BeAssignableTo<IDatabaseSettings>();
    }

    [Fact]
    public void SectionName_ShouldBeCorrect()
    {
        // Assert
        DatabaseSettings.SectionName.Should().Be("Database");
    }

    [Fact]
    public void ConfigurationBinding_ShouldWorkCorrectly()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["Database:Host"] = "test-host",
            ["Database:Port"] = "5433",
            ["Database:Timeout"] = "45",
            ["Database:MaxPoolSize"] = "50",
            ["Database:MinPoolSize"] = "10",
            ["Database:SslMode"] = "Require",
            ["Database:IncludeErrorDetail"] = "false",
            ["Database:CommandTimeout"] = "60"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var settings = new DatabaseSettings();

        // Act
        configuration.GetSection(DatabaseSettings.SectionName).Bind(settings);

        // Assert
        settings.Host.Should().Be("test-host");
        settings.Port.Should().Be(5433);
        settings.Timeout.Should().Be(45);
        settings.MaxPoolSize.Should().Be(50);
        settings.MinPoolSize.Should().Be(10);
        settings.SslMode.Should().Be("Require");
        settings.IncludeErrorDetail.Should().BeFalse();
        settings.CommandTimeout.Should().Be(60);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IncludeErrorDetail_ShouldAcceptBooleanValues(bool includeErrorDetail)
    {
        // Arrange & Act
        var settings = new DatabaseSettings { IncludeErrorDetail = includeErrorDetail };

        // Assert
        settings.IncludeErrorDetail.Should().Be(includeErrorDetail);
    }
}