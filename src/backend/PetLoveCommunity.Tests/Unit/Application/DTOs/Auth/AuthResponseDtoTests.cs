using FluentAssertions;
using PetLoveCommunity.Application.DTOs.Auth;
using System.Text.Json;

namespace PetLoveCommunity.Tests.Unit.Application.DTOs.Auth;

public class AuthResponseDtoTests
{
    #region Constructor and Property Tests

    [Fact]
    public void AuthResponseDto_DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Act
        var dto = new AuthResponseDto();

        // Assert
        dto.Token.Should().BeEmpty();
        dto.ExpiresAt.Should().Be(default(DateTime));
        dto.User.Should().NotBeNull();
        dto.User.Id.Should().Be(Guid.Empty);
        dto.User.FirstName.Should().BeEmpty();
        dto.User.LastName.Should().BeEmpty();
        dto.User.Email.Should().BeEmpty();
    }

    [Fact]
    public void AuthResponseDto_WithValidData_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        var expiresAt = DateTime.UtcNow.AddHours(24);
        var user = CreateTestUserDto();

        // Act
        var dto = new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = user
        };

        // Assert
        dto.Token.Should().Be(token);
        dto.ExpiresAt.Should().Be(expiresAt);
        dto.User.Should().Be(user);
    }

    #endregion

    #region Serialization Tests

    [Fact]
    public void AuthResponseDto_JsonSerialization_ShouldWork()
    {
        // Arrange
        var dto = new AuthResponseDto
        {
            Token = "test-token",
            ExpiresAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            User = CreateTestUserDto()
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<AuthResponseDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Token.Should().Be("test-token");
        deserialized.ExpiresAt.Should().Be(new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        deserialized.User.Should().NotBeNull();
        deserialized.User.Id.Should().Be(dto.User.Id);
        deserialized.User.Email.Should().Be(dto.User.Email);
    }

    [Fact]
    public void AuthResponseDto_WithNullUser_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new AuthResponseDto
        {
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow,
            User = null!
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<AuthResponseDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Token.Should().Be("test-token");
        deserialized.User.Should().BeNull();
    }

    #endregion

    #region UserDto Tests

    [Fact]
    public void UserDto_DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Act
        var dto = new UserDto();

        // Assert
        dto.Id.Should().Be(Guid.Empty);
        dto.FirstName.Should().BeEmpty();
        dto.LastName.Should().BeEmpty();
        dto.Email.Should().BeEmpty();
        dto.PhoneNumber.Should().BeNull();
        dto.Bio.Should().BeNull();
        dto.ProfilePictureUrl.Should().BeNull();
        dto.Role.Should().BeEmpty();
        dto.Status.Should().BeEmpty();
        dto.IsEmailVerified.Should().BeFalse();
        dto.LastLoginAt.Should().BeNull();
        dto.CreatedAt.Should().Be(default(DateTime));
    }

    [Fact]
    public void UserDto_WithAllProperties_ShouldSetCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lastLoginAt = DateTime.UtcNow.AddDays(-1);
        var createdAt = DateTime.UtcNow.AddDays(-30);

        // Act
        var dto = new UserDto
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            PhoneNumber = "+1234567890",
            Bio = "Test bio",
            ProfilePictureUrl = "https://example.com/avatar.jpg",
            Role = "Premium",
            Status = "Active",
            IsEmailVerified = true,
            LastLoginAt = lastLoginAt,
            CreatedAt = createdAt
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.FirstName.Should().Be("John");
        dto.LastName.Should().Be("Doe");
        dto.Email.Should().Be("john.doe@test.com");
        dto.PhoneNumber.Should().Be("+1234567890");
        dto.Bio.Should().Be("Test bio");
        dto.ProfilePictureUrl.Should().Be("https://example.com/avatar.jpg");
        dto.Role.Should().Be("Premium");
        dto.Status.Should().Be("Active");
        dto.IsEmailVerified.Should().BeTrue();
        dto.LastLoginAt.Should().Be(lastLoginAt);
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void UserDto_JsonSerialization_ShouldWork()
    {
        // Arrange
        var dto = CreateTestUserDto();

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<UserDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(dto.Id);
        deserialized.FirstName.Should().Be(dto.FirstName);
        deserialized.LastName.Should().Be(dto.LastName);
        deserialized.Email.Should().Be(dto.Email);
        deserialized.PhoneNumber.Should().Be(dto.PhoneNumber);
        deserialized.Bio.Should().Be(dto.Bio);
        deserialized.ProfilePictureUrl.Should().Be(dto.ProfilePictureUrl);
        deserialized.Role.Should().Be(dto.Role);
        deserialized.Status.Should().Be(dto.Status);
        deserialized.IsEmailVerified.Should().Be(dto.IsEmailVerified);
        deserialized.LastLoginAt.Should().Be(dto.LastLoginAt);
        deserialized.CreatedAt.Should().Be(dto.CreatedAt);
    }

    [Fact]
    public void UserDto_WithNullOptionalFields_ShouldWork()
    {
        // Arrange
        var dto = new UserDto
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            PhoneNumber = null,
            Bio = null,
            ProfilePictureUrl = null,
            Role = "Free",
            Status = "Active",
            IsEmailVerified = false,
            LastLoginAt = null,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<UserDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.PhoneNumber.Should().BeNull();
        deserialized.Bio.Should().BeNull();
        deserialized.ProfilePictureUrl.Should().BeNull();
        deserialized.LastLoginAt.Should().BeNull();
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void AuthResponseDto_WithLongToken_ShouldWork()
    {
        // Arrange
        var longToken = new string('a', 2000); // Very long token
        var dto = new AuthResponseDto
        {
            Token = longToken,
            ExpiresAt = DateTime.UtcNow,
            User = CreateTestUserDto()
        };

        // Act & Assert
        dto.Token.Should().Be(longToken);
        dto.Token.Length.Should().Be(2000);
    }

    [Fact]
    public void AuthResponseDto_WithFutureExpirationDate_ShouldWork()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddYears(10);
        var dto = new AuthResponseDto
        {
            Token = "test-token",
            ExpiresAt = futureDate,
            User = CreateTestUserDto()
        };

        // Act & Assert
        dto.ExpiresAt.Should().Be(futureDate);
        dto.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void AuthResponseDto_WithPastExpirationDate_ShouldWork()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var dto = new AuthResponseDto
        {
            Token = "expired-token",
            ExpiresAt = pastDate,
            User = CreateTestUserDto()
        };

        // Act & Assert
        dto.ExpiresAt.Should().Be(pastDate);
        dto.ExpiresAt.Should().BeBefore(DateTime.UtcNow);
    }

    [Fact]
    public void UserDto_WithSpecialCharacters_ShouldWork()
    {
        // Arrange
        var dto = new UserDto
        {
            Id = Guid.NewGuid(),
            FirstName = "José María",
            LastName = "González-Pérez",
            Email = "josé.maría@domain.com",
            Bio = "Bio with émojis 🎉 and spëcial châracTers",
            Role = "Premium",
            Status = "Active",
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<UserDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.FirstName.Should().Be("José María");
        deserialized.LastName.Should().Be("González-Pérez");
        deserialized.Email.Should().Be("josé.maría@domain.com");
        deserialized.Bio.Should().Be("Bio with émojis 🎉 and spëcial châracTers");
    }

    [Fact]
    public void AuthResponseDto_EmptyToken_ShouldWork()
    {
        // Arrange
        var dto = new AuthResponseDto
        {
            Token = "",
            ExpiresAt = DateTime.UtcNow,
            User = CreateTestUserDto()
        };

        // Act & Assert
        dto.Token.Should().BeEmpty();
        dto.Token.Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private static UserDto CreateTestUserDto()
    {
        return new UserDto
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            PhoneNumber = "+1234567890",
            Bio = "Test user bio",
            ProfilePictureUrl = "https://example.com/avatar.jpg",
            Role = "Free",
            Status = "Active",
            IsEmailVerified = true,
            LastLoginAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
    }

    #endregion
}