using FluentAssertions;
using Moq;
using PetLoveCommunity.API.Configuration;
using PetLoveCommunity.API.Services;
using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.Tests.Unit.API.Services;

public class ConfigurationValidatorTests
{
    private readonly ConfigurationValidator _validator;

    public ConfigurationValidatorTests()
    {
        _validator = new ConfigurationValidator();
    }

    [Fact]
    public void ValidateAndThrow_WithValidObject_ShouldNotThrow()
    {
        // Arrange
        var validSettings = new JwtSettings
        {
            Key = "12345678901234567890123456789012",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationHours = 24
        };

        // Act & Assert
        var action = () => _validator.ValidateAndThrow(validSettings, "JWT Settings");
        action.Should().NotThrow();
    }

    [Fact]
    public void ValidateAndThrow_WithInvalidObject_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invalidSettings = new JwtSettings
        {
            Key = "short", // Too short
            Issuer = "", // Required but empty
            Audience = "TestAudience",
            ExpirationHours = 0 // Out of range
        };

        // Act & Assert
        var action = () => _validator.ValidateAndThrow(invalidSettings, "JWT Settings");
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("JWT Settings configuration validation failed:*");
    }

    [Fact]
    public void ValidateAndThrow_WithInvalidObject_ShouldIncludeAllValidationErrors()
    {
        // Arrange
        var invalidSettings = new JwtSettings
        {
            Key = "short", // Too short
            Issuer = "", // Required but empty
            Audience = "", // Required but empty
            ExpirationHours = 0 // Out of range
        };

        // Act & Assert
        var action = () => _validator.ValidateAndThrow(invalidSettings, "JWT Settings");
        var exception = action.Should().Throw<InvalidOperationException>().Which;
        
        exception.Message.Should().Contain("JWT Settings configuration validation failed:");
        exception.Message.Should().Contain("JWT Key must be at least 32 characters");
        exception.Message.Should().Contain("JWT Issuer is required");
        exception.Message.Should().Contain("JWT Audience is required");
        exception.Message.Should().Contain("between 1 and 168 hours");
    }

    [Fact]
    public void Validate_WithValidObject_ShouldReturnEmptyResults()
    {
        // Arrange
        var validSettings = new DatabaseSettings
        {
            Host = "localhost",
            Port = 5432,
            Timeout = 30,
            MaxPoolSize = 20,
            MinPoolSize = 5,
            SslMode = "Prefer",
            CommandTimeout = 30
        };

        // Act
        var results = _validator.Validate(validSettings);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithInvalidObject_ShouldReturnValidationResults()
    {
        // Arrange
        var invalidSettings = new DatabaseSettings
        {
            Host = "", // Required but empty
            Port = 0, // Out of range
            Timeout = -1, // Out of range
            MaxPoolSize = 0, // Out of range
            MinPoolSize = -1, // Out of range
            SslMode = "", // Required but empty
            CommandTimeout = 0 // Out of range
        };

        // Act
        var results = _validator.Validate(invalidSettings);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().HaveCountGreaterThan(5); // Multiple validation errors
        results.Should().Contain(r => r.ErrorMessage!.Contains("Database host is required"));
        results.Should().Contain(r => r.ErrorMessage!.Contains("Database port must be between"));
        results.Should().Contain(r => r.ErrorMessage!.Contains("SSL mode is required"));
    }

    [Fact]
    public void Validate_WithDatabaseCredentials_ShouldValidateCorrectly()
    {
        // Arrange
        var invalidCredentials = new DatabaseCredentials
        {
            Username = "", // Required but empty
            Password = "short", // Too short
            DatabaseName = "" // Required but empty
        };

        // Act
        var results = _validator.Validate(invalidCredentials);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().HaveCount(3); // Username required, Password min length, DatabaseName required
        results.Should().Contain(r => r.ErrorMessage!.Contains("username") && r.ErrorMessage.Contains("required"));
        // Note: Password with "short" value only triggers MinLength validation, not Required
        results.Should().Contain(r => r.ErrorMessage!.Contains("password") && r.ErrorMessage.Contains("at least 8 characters"));
        results.Should().Contain(r => r.ErrorMessage!.Contains("name") && r.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_WithDatabaseAdminCredentials_ShouldValidateEmailFormat()
    {
        // Arrange
        var invalidAdminCredentials = new DatabaseAdminCredentials
        {
            Email = "invalid-email", // Invalid email format
            Password = "short" // Too short
        };

        // Act
        var results = _validator.Validate(invalidAdminCredentials);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().HaveCount(2);
        results.Should().Contain(r => r.ErrorMessage!.Contains("valid email"));
        results.Should().Contain(r => r.ErrorMessage!.Contains("at least 8 characters"));
    }

    [Theory]
    [InlineData("Test Configuration")]
    [InlineData("JWT Settings")]
    [InlineData("Database Configuration")]
    [InlineData("Custom Config Name")]
    public void ValidateAndThrow_ShouldIncludeConfigurationNameInErrorMessage(string configName)
    {
        // Arrange
        var invalidSettings = new JwtSettings
        {
            Key = "", // Invalid
            Issuer = "",
            Audience = "",
            ExpirationHours = 0
        };

        // Act & Assert
        var action = () => _validator.ValidateAndThrow(invalidSettings, configName);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage($"{configName} configuration validation failed:*");
    }

    [Fact]
    public void ValidateAndThrow_WithNullObject_ShouldThrowArgumentException()
    {
        // Arrange
        JwtSettings? nullSettings = null;

        // Act & Assert
        var action = () => _validator.ValidateAndThrow(nullSettings!, "JWT Settings");
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_WithNullObject_ShouldThrowArgumentException()
    {
        // Arrange
        JwtSettings? nullSettings = null;

        // Act & Assert
        var action = () => _validator.Validate(nullSettings!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateAndThrow_WithComplexValidObject_ShouldNotThrow()
    {
        // Arrange
        var validCredentials = new DatabaseCredentials
        {
            Username = "petlove_user",
            Password = "securePassword123!",
            DatabaseName = "petlovecommunity_db"
        };

        // Act & Assert
        var action = () => _validator.ValidateAndThrow(validCredentials, "Database Credentials");
        action.Should().NotThrow();
    }

    [Fact]
    public void Validate_ShouldValidateAllProperties()
    {
        // Arrange
        var partiallyInvalidSettings = new DatabaseSettings
        {
            Host = "valid-host", // Valid
            Port = 5432, // Valid
            Timeout = 30, // Valid
            MaxPoolSize = 2000, // Invalid - out of range
            MinPoolSize = 5, // Valid
            SslMode = "Prefer", // Valid
            CommandTimeout = 30 // Valid
        };

        // Act
        var results = _validator.Validate(partiallyInvalidSettings);

        // Assert
        results.Should().HaveCount(1);
        results.Should().Contain(r => r.ErrorMessage!.Contains("Max pool size must be between 1 and 1000"));
    }

    [Fact]
    public void ImplementsInterface_ShouldImplementIConfigurationValidator()
    {
        // Assert
        _validator.Should().BeAssignableTo<IConfigurationValidator>();
    }

    [Fact]
    public void ValidateAndThrow_WithValidAdminCredentials_ShouldNotThrow()
    {
        // Arrange
        var validAdminCredentials = new DatabaseAdminCredentials
        {
            Email = "admin@petlove.com",
            Password = "adminPassword123!"
        };

        // Act & Assert
        var action = () => _validator.ValidateAndThrow(validAdminCredentials, "Database Admin");
        action.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithMinimumValidValues_ShouldPass()
    {
        // Arrange - Test boundary conditions
        var boundarySettings = new DatabaseSettings
        {
            Host = "h", // Minimum valid host
            Port = 1, // Minimum valid port
            Timeout = 1, // Minimum valid timeout
            MaxPoolSize = 1, // Minimum valid max pool size
            MinPoolSize = 0, // Minimum valid min pool size
            SslMode = "D", // Minimum valid SSL mode
            CommandTimeout = 1 // Minimum valid command timeout
        };

        // Act
        var results = _validator.Validate(boundarySettings);

        // Assert
        results.Should().BeEmpty();
    }
}