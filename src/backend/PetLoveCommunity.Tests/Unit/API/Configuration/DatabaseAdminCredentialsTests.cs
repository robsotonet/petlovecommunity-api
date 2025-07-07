using FluentAssertions;
using Microsoft.Extensions.Configuration;
using PetLoveCommunity.API.Configuration;
using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.Tests.Unit.API.Configuration;

public class DatabaseAdminCredentialsTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var credentials = new DatabaseAdminCredentials();

        // Assert
        credentials.Email.Should().BeEmpty();
        credentials.Password.Should().BeEmpty();
    }

    [Fact]
    public void Properties_ShouldBeImmutable_WhenUsingInit()
    {
        // Arrange
        var credentials = new DatabaseAdminCredentials
        {
            Email = "admin@test.com",
            Password = "test-password"
        };

        // Assert - Properties should not be settable after initialization
        credentials.Email.Should().Be("admin@test.com");
        credentials.Password.Should().Be("test-password");
    }

    [Theory]
    [InlineData("", false)] // Empty email
    [InlineData("   ", false)] // Whitespace only
    [InlineData("invalid-email", false)] // Invalid email format
    [InlineData("invalid@", false)] // Incomplete email
    [InlineData("@invalid.com", false)] // Missing local part
    [InlineData("valid@example.com", true)] // Valid email
    [InlineData("admin@petlove.com", true)] // Valid email
    [InlineData("user.name+tag@domain.co.uk", true)] // Complex valid email
    public void Email_Validation_ShouldRespectEmailAttribute(string email, bool isValid)
    {
        // Arrange
        var credentials = new DatabaseAdminCredentials { Email = email };
        var context = new ValidationContext(credentials) { MemberName = nameof(DatabaseAdminCredentials.Email) };
        var results = new List<ValidationResult>();

        // Act
        var actualIsValid = Validator.TryValidateProperty(credentials.Email, context, results);

        // Assert
        actualIsValid.Should().Be(isValid);
        if (!isValid)
        {
            results.Should().HaveCountGreaterThan(0);
            if (string.IsNullOrWhiteSpace(email))
            {
                results.Should().Contain(r => r.ErrorMessage!.Contains("required"));
            }
            else
            {
                results.Should().Contain(r => r.ErrorMessage!.Contains("valid email"));
            }
        }
    }

    [Theory]
    [InlineData("", false)] // Empty password
    [InlineData("1234567", false)] // Too short (7 chars)
    [InlineData("12345678", true)] // Exactly 8 chars (minimum)
    [InlineData("longerpassword", true)] // Longer than minimum
    [InlineData("AdminPass123!", true)] // Complex password
    public void Password_Validation_ShouldRespectMinLengthAttribute(string password, bool isValid)
    {
        // Arrange
        var credentials = new DatabaseAdminCredentials { Password = password };
        var context = new ValidationContext(credentials) { MemberName = nameof(DatabaseAdminCredentials.Password) };
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

    [Fact]
    public void ValidObject_ShouldPassCompleteValidation()
    {
        // Arrange
        var credentials = new DatabaseAdminCredentials
        {
            Email = "admin@petlove.com",
            Password = "adminpassword123"
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
        var credentials = new DatabaseAdminCredentials
        {
            Email = "invalid-email", // Invalid - not a valid email format
            Password = "short" // Invalid - too short
        };
        var context = new ValidationContext(credentials);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(credentials, context, results, validateAllProperties: true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().HaveCount(2); // Email format + password length
        results.Should().Contain(r => r.ErrorMessage!.Contains("valid email"));
        results.Should().Contain(r => r.ErrorMessage!.Contains("at least 8 characters"));
    }

    [Fact]
    public void ImplementsInterface_ShouldImplementIDatabaseAdminCredentials()
    {
        // Arrange & Act
        var credentials = new DatabaseAdminCredentials();

        // Assert
        credentials.Should().BeAssignableTo<IDatabaseAdminCredentials>();
    }

    [Fact]
    public void SectionName_ShouldBeCorrect()
    {
        // Assert
        DatabaseAdminCredentials.SectionName.Should().Be("DatabaseAdmin");
    }

    [Fact]
    public void ConfigurationBinding_ShouldWorkCorrectly()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["DatabaseAdmin:Email"] = "test@admin.com",
            ["DatabaseAdmin:Password"] = "testadminpass123"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var credentials = new DatabaseAdminCredentials();

        // Act
        configuration.GetSection(DatabaseAdminCredentials.SectionName).Bind(credentials);

        // Assert
        credentials.Email.Should().Be("test@admin.com");
        credentials.Password.Should().Be("testadminpass123");
    }

    [Fact]
    public void Email_ShouldHandleVariousValidFormats()
    {
        // Arrange
        var validEmails = new[]
        {
            "simple@example.com",
            "very.common@example.com",
            "disposable.style.email.with+symbol@example.com",
            "user.name+tag+sorting@example.com",
            "admin@localhost",
            "test@test-domain.com",
            "user@domain.co.uk"
        };

        foreach (var email in validEmails)
        {
            // Act
            var credentials = new DatabaseAdminCredentials
            {
                Email = email,
                Password = "validpassword123"
            };

            var context = new ValidationContext(credentials);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(credentials, context, results, validateAllProperties: true);

            // Assert
            isValid.Should().BeTrue($"Email '{email}' should be valid");
            results.Should().BeEmpty($"Email '{email}' should pass validation");
        }
    }

    [Fact]
    public void Password_ShouldHandleSpecialCharacters()
    {
        // Arrange & Act
        var credentials = new DatabaseAdminCredentials
        {
            Email = "admin@petlove.com",
            Password = "Adm!n@P@ssw0rd#2024$%^"
        };

        // Assert
        credentials.Password.Should().Be("Adm!n@P@ssw0rd#2024$%^");
        
        // Validate it passes validation
        var context = new ValidationContext(credentials);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(credentials, context, results, validateAllProperties: true);
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("admin@company.com", "AdminPass123")]
    [InlineData("dba@petlove.com", "DatabaseAdmin2024!")]
    [InlineData("superuser@localhost", "VerySecurePassword")]
    public void ValidCredentialsCombinations_ShouldPassValidation(string email, string password)
    {
        // Arrange
        var credentials = new DatabaseAdminCredentials
        {
            Email = email,
            Password = password
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
    public void EmptyEmailAndPassword_ShouldFailValidation()
    {
        // Arrange
        var credentials = new DatabaseAdminCredentials
        {
            Email = "",
            Password = ""
        };
        var context = new ValidationContext(credentials);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(credentials, context, results, validateAllProperties: true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().HaveCount(2); // Both email and password should fail
        results.Should().Contain(r => r.ErrorMessage!.Contains("email") && r.ErrorMessage.Contains("required"));
        results.Should().Contain(r => r.ErrorMessage!.Contains("password") && r.ErrorMessage.Contains("required"));
    }
}