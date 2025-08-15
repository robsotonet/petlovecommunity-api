using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using PetLoveCommunity.Application.Configuration;
using PetLoveCommunity.Application.Services;
using PetLoveCommunity.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PetLoveCommunity.Tests.Unit.Application.Services;

public class JwtServiceTests
{
    private readonly Mock<IJwtSettings> _mockJwtSettings;
    private readonly JwtService _jwtService;
    private const string TestKey = "ThisIsAVeryLongSecretKeyForTestingThatIsAtLeast32CharactersLong";
    private const string TestIssuer = "TestIssuer";
    private const string TestAudience = "TestAudience";
    private const int TestExpirationHours = 24;

    public JwtServiceTests()
    {
        _mockJwtSettings = new Mock<IJwtSettings>();
        SetupValidJwtSettings();
        _jwtService = new JwtService(_mockJwtSettings.Object);
    }

    private void SetupValidJwtSettings()
    {
        _mockJwtSettings.Setup(x => x.Key).Returns(TestKey);
        _mockJwtSettings.Setup(x => x.Issuer).Returns(TestIssuer);
        _mockJwtSettings.Setup(x => x.Audience).Returns(TestAudience);
        _mockJwtSettings.Setup(x => x.ExpirationHours).Returns(TestExpirationHours);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullJwtSettings_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new JwtService(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("jwtSettings");
    }

    [Fact]
    public void Constructor_WithValidJwtSettings_ShouldCreateInstance()
    {
        // Act
        var service = new JwtService(_mockJwtSettings.Object);

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region GenerateJwtToken Tests

    [Fact]
    public void GenerateJwtToken_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => _jwtService.GenerateJwtToken(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("user");
    }

    [Fact]
    public void GenerateJwtToken_WithValidUser_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _jwtService.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().Match("*.*.*", "JWT should have three parts separated by dots");
    }

    [Fact]
    public void GenerateJwtToken_WithValidUser_ShouldContainCorrectClaims()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _jwtService.GenerateJwtToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        // JWT tokens use the short form claim names when serialized
        jsonToken.Claims.Should().Contain(c => 
            c.Type == "nameid" && c.Value == user.Id.ToString());
        jsonToken.Claims.Should().Contain(c => 
            c.Type == "email" && c.Value == user.Email!);
        jsonToken.Claims.Should().Contain(c => 
            c.Type == "given_name" && c.Value == user.FirstName!);
        jsonToken.Claims.Should().Contain(c => 
            c.Type == "family_name" && c.Value == user.LastName!);
        jsonToken.Claims.Should().Contain(c => 
            c.Type == "role" && c.Value == user.Role.ToString());
        jsonToken.Claims.Should().Contain(c => 
            c.Type == "status" && c.Value == user.Status.ToString());
        jsonToken.Claims.Should().Contain(c => 
            c.Type == "isEmailVerified" && c.Value == user.IsEmailVerified.ToString());
    }

    [Fact]
    public void GenerateJwtToken_WithValidUser_ShouldHaveCorrectIssuerAndAudience()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _jwtService.GenerateJwtToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        jsonToken.Issuer.Should().Be(TestIssuer);
        jsonToken.Audiences.Should().Contain(TestAudience);
    }

    [Fact]
    public void GenerateJwtToken_WithValidUser_ShouldHaveCorrectExpiration()
    {
        // Arrange
        var user = CreateTestUser();
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _jwtService.GenerateJwtToken(user);

        // Assert
        var afterGeneration = DateTime.UtcNow;
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        var expectedMinExpiry = beforeGeneration.AddHours(TestExpirationHours);
        var expectedMaxExpiry = afterGeneration.AddHours(TestExpirationHours);

        jsonToken.ValidTo.Should().BeAfter(expectedMinExpiry.AddSeconds(-1));
        jsonToken.ValidTo.Should().BeBefore(expectedMaxExpiry.AddSeconds(1));
    }

    [Fact]
    public void GenerateJwtToken_WithValidUser_ShouldBeValidatableWithCorrectKey()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _jwtService.GenerateJwtToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = TestIssuer,
            ValidAudience = TestAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(TestKey)),
            ClockSkew = TimeSpan.Zero
        };

        var act = () => tokenHandler.ValidateToken(token, validationParameters, out _);
        act.Should().NotThrow("token should be valid with correct parameters");
    }

    [Fact]
    public void GenerateJwtToken_WithDifferentUsers_ShouldGenerateDifferentTokens()
    {
        // Arrange
        var user1 = CreateTestUser();
        var user2 = CreateTestUser();
        user2.Id = Guid.NewGuid();
        user2.Email = "different@test.com";

        // Act
        var token1 = _jwtService.GenerateJwtToken(user1);
        var token2 = _jwtService.GenerateJwtToken(user2);

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateJwtToken_WithSameUserCalledTwice_ShouldGenerateDifferentTokens()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token1 = _jwtService.GenerateJwtToken(user);
        System.Threading.Thread.Sleep(1000); // Ensure different timestamps
        var token2 = _jwtService.GenerateJwtToken(user);

        // Assert
        token1.Should().NotBe(token2, "tokens should differ due to different generation times");
    }

    [Theory]
    [InlineData(UserRole.Free)]
    [InlineData(UserRole.Premium)]
    [InlineData(UserRole.Vendor)]
    [InlineData(UserRole.Shelter)]
    [InlineData(UserRole.Admin)]
    public void GenerateJwtToken_WithDifferentUserRoles_ShouldIncludeCorrectRoleClaim(UserRole role)
    {
        // Arrange
        var user = CreateTestUser();
        user.Role = role;

        // Act
        var token = _jwtService.GenerateJwtToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        jsonToken.Claims.Should().Contain(c => 
            c.Type == "role" && c.Value == role.ToString());
    }

    [Theory]
    [InlineData(UserStatus.Active)]
    [InlineData(UserStatus.Inactive)]
    [InlineData(UserStatus.Suspended)]
    [InlineData(UserStatus.Banned)]
    public void GenerateJwtToken_WithDifferentUserStatuses_ShouldIncludeCorrectStatusClaim(UserStatus status)
    {
        // Arrange
        var user = CreateTestUser();
        user.Status = status;

        // Act
        var token = _jwtService.GenerateJwtToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        jsonToken.Claims.Should().Contain(c => 
            c.Type == "status" && c.Value == status.ToString());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GenerateJwtToken_WithDifferentEmailVerificationStatus_ShouldIncludeCorrectClaim(bool isEmailVerified)
    {
        // Arrange
        var user = CreateTestUser();
        user.IsEmailVerified = isEmailVerified;

        // Act
        var token = _jwtService.GenerateJwtToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        jsonToken.Claims.Should().Contain(c => 
            c.Type == "isEmailVerified" && c.Value == isEmailVerified.ToString());
    }

    [Fact]
    public void GenerateJwtToken_WithSpecialCharactersInUserData_ShouldHandleCorrectly()
    {
        // Arrange
        var user = CreateTestUser();
        user.FirstName = "José María";
        user.LastName = "González-Pérez";
        user.Email = "josé.maría@test.com";

        // Act
        var token = _jwtService.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        jsonToken.Claims.Should().Contain(c => 
            c.Type == "given_name" && c.Value == "José María");
        jsonToken.Claims.Should().Contain(c => 
            c.Type == "family_name" && c.Value == "González-Pérez");
        jsonToken.Claims.Should().Contain(c => 
            c.Type == "email" && c.Value == "josé.maría@test.com");
    }

    #endregion

    #region JWT Settings Validation Tests

    [Fact]
    public void GenerateJwtToken_WithInvalidKey_ShouldThrowException()
    {
        // Arrange
        _mockJwtSettings.Setup(x => x.Key).Returns(""); // Empty key
        var user = CreateTestUser();
        var service = new JwtService(_mockJwtSettings.Object);

        // Act & Assert
        var act = () => service.GenerateJwtToken(user);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GenerateJwtToken_WithInvalidIssuer_ShouldStillGenerateToken(string? invalidIssuer)
    {
        // Arrange
        _mockJwtSettings.Setup(x => x.Issuer).Returns(invalidIssuer!);
        var user = CreateTestUser();
        var service = new JwtService(_mockJwtSettings.Object);

        // Act
        var token = service.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GenerateJwtToken_WithInvalidAudience_ShouldStillGenerateToken(string? invalidAudience)
    {
        // Arrange
        _mockJwtSettings.Setup(x => x.Audience).Returns(invalidAudience!);
        var user = CreateTestUser();
        var service = new JwtService(_mockJwtSettings.Object);

        // Act
        var token = service.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(168)]
    public void GenerateJwtToken_WithDifferentExpirationHours_ShouldGenerateTokenWithCorrectExpiry(int expirationHours)
    {
        // Arrange
        _mockJwtSettings.Setup(x => x.ExpirationHours).Returns(expirationHours);
        var user = CreateTestUser();
        var service = new JwtService(_mockJwtSettings.Object);
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = service.GenerateJwtToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        var expectedExpiry = beforeGeneration.AddHours(expirationHours);

        jsonToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromMinutes(1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GenerateJwtToken_WithInvalidExpirationHours_ShouldThrowException(int expirationHours)
    {
        // Arrange
        _mockJwtSettings.Setup(x => x.ExpirationHours).Returns(expirationHours);
        var user = CreateTestUser();
        var service = new JwtService(_mockJwtSettings.Object);

        // Act & Assert
        var act = () => service.GenerateJwtToken(user);
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Helper Methods

    private static User CreateTestUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Role = UserRole.Free,
            Status = UserStatus.Active,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    #endregion
}