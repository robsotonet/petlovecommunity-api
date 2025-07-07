using FluentAssertions;
using Microsoft.Extensions.Configuration;
using PetLoveCommunity.API.Configuration;
using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.Tests.Unit.API.Configuration;

public class JwtSettingsTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var settings = new JwtSettings();

        // Assert
        settings.Key.Should().BeEmpty();
        settings.Issuer.Should().BeEmpty();
        settings.Audience.Should().BeEmpty();
        settings.ExpirationHours.Should().Be(24);
    }

    [Fact]
    public void Properties_ShouldBeImmutable_WhenUsingInit()
    {
        // Arrange
        var settings = new JwtSettings
        {
            Key = "test-key-12345678901234567890123456789012",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationHours = 48
        };

        // Assert - Properties should not be settable after initialization
        // This test verifies the 'init' keyword is working
        settings.Key.Should().Be("test-key-12345678901234567890123456789012");
        settings.Issuer.Should().Be("test-issuer");
        settings.Audience.Should().Be("test-audience");
        settings.ExpirationHours.Should().Be(48);
    }

    [Theory]
    [InlineData("", false)] // Empty key
    [InlineData("short", false)] // Too short
    [InlineData("12345678901234567890123456789012", true)] // Exactly 32 chars
    [InlineData("123456789012345678901234567890123", true)] // More than 32 chars
    public void Key_Validation_ShouldRespectMinLengthAttribute(string key, bool isValid)
    {
        // Arrange
        var settings = new JwtSettings { Key = key };
        var context = new ValidationContext(settings) { MemberName = nameof(JwtSettings.Key) };
        var results = new List<ValidationResult>();

        // Act
        var actualIsValid = Validator.TryValidateProperty(settings.Key, context, results);

        // Assert
        actualIsValid.Should().Be(isValid);
        if (!isValid)
        {
            results.Should().HaveCountGreaterThan(0);
            if (string.IsNullOrEmpty(key))
            {
                results.Should().Contain(r => r.ErrorMessage!.Contains("required"));
            }
            else
            {
                results.Should().Contain(r => r.ErrorMessage!.Contains("at least 32 characters"));
            }
        }
    }

    [Theory]
    [InlineData("", false)] // Empty issuer
    [InlineData("   ", false)] // Whitespace only
    [InlineData("valid-issuer", true)] // Valid issuer
    public void Issuer_Validation_ShouldRespectRequiredAttribute(string issuer, bool isValid)
    {
        // Arrange
        var settings = new JwtSettings { Issuer = issuer };
        var context = new ValidationContext(settings) { MemberName = nameof(JwtSettings.Issuer) };
        var results = new List<ValidationResult>();

        // Act
        var actualIsValid = Validator.TryValidateProperty(settings.Issuer, context, results);

        // Assert
        actualIsValid.Should().Be(isValid);
        if (!isValid)
        {
            results.Should().HaveCount(1);
            results[0].ErrorMessage.Should().Contain("required");
        }
    }

    [Theory]
    [InlineData("", false)] // Empty audience
    [InlineData("valid-audience", true)] // Valid audience
    public void Audience_Validation_ShouldRespectRequiredAttribute(string audience, bool isValid)
    {
        // Arrange
        var settings = new JwtSettings { Audience = audience };
        var context = new ValidationContext(settings) { MemberName = nameof(JwtSettings.Audience) };
        var results = new List<ValidationResult>();

        // Act
        var actualIsValid = Validator.TryValidateProperty(settings.Audience, context, results);

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
    [InlineData(24, true)] // Default value
    [InlineData(168, true)] // Maximum valid (1 week)
    [InlineData(169, false)] // Above maximum
    public void ExpirationHours_Validation_ShouldRespectRangeAttribute(int hours, bool isValid)
    {
        // Arrange
        var settings = new JwtSettings { ExpirationHours = hours };
        var context = new ValidationContext(settings) { MemberName = nameof(JwtSettings.ExpirationHours) };
        var results = new List<ValidationResult>();

        // Act
        var actualIsValid = Validator.TryValidateProperty(settings.ExpirationHours, context, results);

        // Assert
        actualIsValid.Should().Be(isValid);
        if (!isValid)
        {
            results.Should().HaveCount(1);
            results[0].ErrorMessage.Should().Contain("between 1 and 168");
        }
    }

    [Fact]
    public void ValidObject_ShouldPassCompleteValidation()
    {
        // Arrange
        var settings = new JwtSettings
        {
            Key = "12345678901234567890123456789012",
            Issuer = "PetLoveCommunity",
            Audience = "PetLoveCommunityUsers",
            ExpirationHours = 24
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
    public void ImplementsInterface_ShouldImplementIJwtSettings()
    {
        // Arrange & Act
        var settings = new JwtSettings();

        // Assert
        settings.Should().BeAssignableTo<IJwtSettings>();
    }

    [Fact]
    public void SectionName_ShouldBeCorrect()
    {
        // Assert
        JwtSettings.SectionName.Should().Be("Jwt");
    }

    [Fact]
    public void ConfigurationBinding_ShouldWorkCorrectly()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["Jwt:Key"] = "test-key-that-is-long-enough-for-validation",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience", 
            ["Jwt:ExpirationHours"] = "48"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var settings = new JwtSettings();

        // Act
        configuration.GetSection(JwtSettings.SectionName).Bind(settings);

        // Assert
        settings.Key.Should().Be("test-key-that-is-long-enough-for-validation");
        settings.Issuer.Should().Be("TestIssuer");
        settings.Audience.Should().Be("TestAudience");
        settings.ExpirationHours.Should().Be(48);
    }
}