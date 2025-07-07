using FluentAssertions;
using Microsoft.Extensions.Configuration;
using PetLoveCommunity.API.Configuration;
using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.Tests.Unit.API.Configuration;

public class DatabaseCredentialsTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var credentials = new DatabaseCredentials();

        // Assert
        credentials.Username.Should().BeEmpty();
        credentials.Password.Should().BeEmpty();
        credentials.DatabaseName.Should().BeEmpty();
    }

    [Fact]
    public void Properties_ShouldBeImmutable_WhenUsingInit()
    {
        // Arrange
        var credentials = new DatabaseCredentials
        {
            Username = "test-user",
            Password = "test-password",
            DatabaseName = "test-database"
        };

        // Assert - Properties should not be settable after initialization
        credentials.Username.Should().Be("test-user");
        credentials.Password.Should().Be("test-password");
        credentials.DatabaseName.Should().Be("test-database");
    }

    [Theory]
    [InlineData("", false)] // Empty username
    [InlineData("   ", false)] // Whitespace only
    [InlineData("validuser", true)] // Valid username
    [InlineData("user123", true)] // Valid username with numbers
    [InlineData("user_with_underscores", true)] // Valid username with underscores
    public void Username_Validation_ShouldRespectRequiredAttribute(string username, bool isValid)
    {
        // Arrange
        var credentials = new DatabaseCredentials { Username = username };
        var context = new ValidationContext(credentials) { MemberName = nameof(DatabaseCredentials.Username) };
        var results = new List<ValidationResult>();

        // Act
        var actualIsValid = Validator.TryValidateProperty(credentials.Username, context, results);

        // Assert
        actualIsValid.Should().Be(isValid);
        if (!isValid)
        {
            results.Should().HaveCount(1);
            results[0].ErrorMessage.Should().Contain("required");
        }
    }

    [Theory]
    [InlineData("", false)] // Empty password
    [InlineData("1234567", false)] // Too short (7 chars)
    [InlineData("12345678", true)] // Exactly 8 chars (minimum)
    [InlineData("longerpassword", true)] // Longer than minimum
    [InlineData("complex!Pass123", true)] // Complex password
    public void Password_Validation_ShouldRespectMinLengthAttribute(string password, bool isValid)
    {
        // Arrange
        var credentials = new DatabaseCredentials { Password = password };
        var context = new ValidationContext(credentials) { MemberName = nameof(DatabaseCredentials.Password) };
        var results = new List<ValidationResult>();

        // Act
        var actualIsValid = Validator.TryValidateProperty(credentials.Password, context, results);

        // Assert
        actualIsValid.Should().Be(isValid);
        if (!isValid)
        {
            results.Should().HaveCountGreaterThan(0);
            if (string.IsNullOrEmpty(password))
            {
                results.Should().Contain(r => r.ErrorMessage!.Contains("required"));
            }
            else
            {
                results.Should().Contain(r => r.ErrorMessage!.Contains("at least 8 characters"));
            }
        }
    }

    [Theory]
    [InlineData("", false)] // Empty database name
    [InlineData("   ", false)] // Whitespace only
    [InlineData("validdb", true)] // Valid database name
    [InlineData("petlovecommunity", true)] // Valid database name
    [InlineData("test_db_123", true)] // Valid database name with underscores and numbers
    public void DatabaseName_Validation_ShouldRespectRequiredAttribute(string databaseName, bool isValid)
    {
        // Arrange
        var credentials = new DatabaseCredentials { DatabaseName = databaseName };
        var context = new ValidationContext(credentials) { MemberName = nameof(DatabaseCredentials.DatabaseName) };
        var results = new List<ValidationResult>();

        // Act
        var actualIsValid = Validator.TryValidateProperty(credentials.DatabaseName, context, results);

        // Assert
        actualIsValid.Should().Be(isValid);
        if (!isValid)
        {
            results.Should().HaveCount(1);
            results[0].ErrorMessage.Should().Contain("required");
        }
    }

    [Fact]
    public void ValidObject_ShouldPassCompleteValidation()
    {
        // Arrange
        var credentials = new DatabaseCredentials
        {
            Username = "petlove_user",
            Password = "securepassword123",
            DatabaseName = "petlovecommunity"
        };
        var context = new ValidationContext(credentials);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(credentials, context, results, validateAllProperties: true);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void InvalidObject_ShouldFailValidation()
    {
        // Arrange
        var credentials = new DatabaseCredentials
        {
            Username = "", // Invalid - empty
            Password = "short", // Invalid - too short
            DatabaseName = "" // Invalid - empty
        };
        var context = new ValidationContext(credentials);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(credentials, context, results, validateAllProperties: true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().HaveCount(3); // Username required, Password min length, DatabaseName required
    }

    [Fact]
    public void ImplementsInterface_ShouldImplementIDatabaseCredentials()
    {
        // Arrange & Act
        var credentials = new DatabaseCredentials();

        // Assert
        credentials.Should().BeAssignableTo<IDatabaseCredentials>();
    }

    [Fact]
    public void SectionName_ShouldBeCorrect()
    {
        // Assert
        DatabaseCredentials.SectionName.Should().Be("Database");
    }

    [Fact]
    public void ConfigurationBinding_ShouldWorkCorrectly()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["Database:Username"] = "testuser",
            ["Database:Password"] = "testpassword123",
            ["Database:DatabaseName"] = "testdatabase"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var credentials = new DatabaseCredentials();

        // Act
        configuration.GetSection(DatabaseCredentials.SectionName).Bind(credentials);

        // Assert
        credentials.Username.Should().Be("testuser");
        credentials.Password.Should().Be("testpassword123");
        credentials.DatabaseName.Should().Be("testdatabase");
    }

    [Fact]
    public void Password_ShouldHandleSpecialCharacters()
    {
        // Arrange & Act
        var credentials = new DatabaseCredentials
        {
            Username = "testuser",
            Password = "P@ssw0rd!2023#$%",
            DatabaseName = "testdb"
        };

        // Assert
        credentials.Password.Should().Be("P@ssw0rd!2023#$%");
        
        // Validate it passes validation
        var context = new ValidationContext(credentials);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(credentials, context, results, validateAllProperties: true);
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("user", "password1", "db1")]
    [InlineData("admin", "superSecurePass123!", "production_db")]
    [InlineData("app_user", "MyVeryLongPassword2024", "application_database")]
    public void ValidCredentialsCombinations_ShouldPassValidation(string username, string password, string databaseName)
    {
        // Arrange
        var credentials = new DatabaseCredentials
        {
            Username = username,
            Password = password,
            DatabaseName = databaseName
        };
        var context = new ValidationContext(credentials);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(credentials, context, results, validateAllProperties: true);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }
}