using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PetLoveCommunity.API.Controllers;
using PetLoveCommunity.Application.DTOs.Auth;
using PetLoveCommunity.Application.DTOs.Shared;
using PetLoveCommunity.Application.Interfaces;
using PetLoveCommunity.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.Tests.Unit.API.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockJwtService = new Mock<IJwtService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockUserService.Object, _mockJwtService.Object, _mockLogger.Object);
        
        // Setup HttpContext for correlation ID functionality
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullUserService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new AuthController(null!, _mockJwtService.Object, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("userService");
    }

    [Fact]
    public void Constructor_WithNullJwtService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new AuthController(_mockUserService.Object, null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("jwtService");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new AuthController(_mockUserService.Object, _mockJwtService.Object, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccessResponse()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123"
        };
        var user = CreateTestUser();
        var token = "jwt-token-here";

        _mockUserService.Setup(x => x.Authenticate(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync(user);
        _mockJwtService.Setup(x => x.GenerateJwtToken(user))
            .Returns(token);

        // Act
        var result = await _controller.LoginAsync(loginRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().Be(token);
        result.Data.User.Should().NotBeNull();
        result.Data.User.Email.Should().Be(user.Email);
        result.Message.Should().Be("Login successful");

        _mockUserService.Verify(x => x.Authenticate(loginRequest.Email, loginRequest.Password), Times.Once);
        _mockJwtService.Verify(x => x.GenerateJwtToken(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ShouldReturnFailureResponse()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        _mockUserService.Setup(x => x.Authenticate(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.LoginAsync(loginRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Invalid email or password");

        _mockUserService.Verify(x => x.Authenticate(loginRequest.Email, loginRequest.Password), Times.Once);
        _mockJwtService.Verify(x => x.GenerateJwtToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldReturnFailureResponse()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123"
        };
        var inactiveUser = CreateTestUser();
        inactiveUser.Status = UserStatus.Inactive;

        _mockUserService.Setup(x => x.Authenticate(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync(inactiveUser);

        // Act
        var result = await _controller.LoginAsync(loginRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Account is not active");

        _mockUserService.Verify(x => x.Authenticate(loginRequest.Email, loginRequest.Password), Times.Once);
        _mockJwtService.Verify(x => x.GenerateJwtToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidModelState_ShouldReturnValidationError()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "",
            Password = "123"
        };

        // Simulate ModelState errors
        _controller.ModelState.AddModelError("Email", "Email is required");
        _controller.ModelState.AddModelError("Password", "Password must be at least 6 characters");

        // Act
        var result = await _controller.LoginAsync(loginRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Contain("Validation failed");
        result.Message.Should().Contain("Email is required");
        result.Message.Should().Contain("Password must be at least 6 characters");

        _mockUserService.Verify(x => x.Authenticate(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenServiceThrowsException_ShouldReturnFailureResponse()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        _mockUserService.Setup(x => x.Authenticate(loginRequest.Email, loginRequest.Password))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.LoginAsync(loginRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("An error occurred during login");

        VerifyErrorLog("Error during login for email: {Email}");
    }

    [Theory]
    [InlineData(UserStatus.Suspended)]
    [InlineData(UserStatus.Banned)]
    public async Task LoginAsync_WithSuspendedOrBannedUser_ShouldReturnFailureResponse(UserStatus userStatus)
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123"
        };
        var user = CreateTestUser();
        user.Status = userStatus;

        _mockUserService.Setup(x => x.Authenticate(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.LoginAsync(loginRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Account is not active");
    }

    #endregion

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldReturnSuccessResponse()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "password123",
            ConfirmPassword = "password123",
            PhoneNumber = "+1234567890"
        };
        var token = "jwt-token-here";

        _mockUserService.Setup(x => x.GetUserByEmail(registerRequest.Email))
            .ReturnsAsync((User?)null);
        _mockUserService.Setup(x => x.Register(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _mockJwtService.Setup(x => x.GenerateJwtToken(It.IsAny<User>()))
            .Returns(token);

        // Act
        var result = await _controller.RegisterAsync(registerRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().Be(token);
        result.Data.User.Should().NotBeNull();
        result.Data.User.Email.Should().Be(registerRequest.Email);
        result.Data.User.FirstName.Should().Be(registerRequest.FirstName);
        result.Message.Should().Be("Registration successful");

        _mockUserService.Verify(x => x.GetUserByEmail(registerRequest.Email), Times.Once);
        _mockUserService.Verify(x => x.Register(It.IsAny<User>()), Times.Once);
        _mockJwtService.Verify(x => x.GenerateJwtToken(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnFailureResponse()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };
        var existingUser = CreateTestUser();

        _mockUserService.Setup(x => x.GetUserByEmail(registerRequest.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _controller.RegisterAsync(registerRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("User with this email already exists");

        _mockUserService.Verify(x => x.GetUserByEmail(registerRequest.Email), Times.Once);
        _mockUserService.Verify(x => x.Register(It.IsAny<User>()), Times.Never);
        _mockJwtService.Verify(x => x.GenerateJwtToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidModelState_ShouldReturnValidationError()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            FirstName = "",
            LastName = "",
            Email = "invalid-email",
            Password = "123",
            ConfirmPassword = "456"
        };

        // Simulate ModelState errors
        _controller.ModelState.AddModelError("FirstName", "FirstName is required");
        _controller.ModelState.AddModelError("Password", "Password must be at least 6 characters");

        // Act
        var result = await _controller.RegisterAsync(registerRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Contain("Validation failed");
        result.Message.Should().Contain("FirstName is required");

        _mockUserService.Verify(x => x.GetUserByEmail(It.IsAny<string>()), Times.Never);
        _mockUserService.Verify(x => x.Register(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenRegisterThrowsInvalidOperationException_ShouldReturnFailureResponse()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        _mockUserService.Setup(x => x.GetUserByEmail(registerRequest.Email))
            .ReturnsAsync((User?)null);
        _mockUserService.Setup(x => x.Register(It.IsAny<User>()))
            .ThrowsAsync(new InvalidOperationException("User already exists"));

        // Act
        var result = await _controller.RegisterAsync(registerRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("User already exists");

        VerifyWarningLog("Registration failed for email: {Email}");
    }

    [Fact]
    public async Task RegisterAsync_WhenServiceThrowsException_ShouldReturnFailureResponse()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        _mockUserService.Setup(x => x.GetUserByEmail(registerRequest.Email))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.RegisterAsync(registerRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("An error occurred during registration");

        VerifyErrorLog("Error during registration for email: {Email}");
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserWithCorrectProperties()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "password123",
            ConfirmPassword = "password123",
            PhoneNumber = "+1234567890"
        };

        User capturedUser = null!;
        _mockUserService.Setup(x => x.GetUserByEmail(registerRequest.Email))
            .ReturnsAsync((User?)null);
        _mockUserService.Setup(x => x.Register(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user)
            .Returns(Task.CompletedTask);
        _mockJwtService.Setup(x => x.GenerateJwtToken(It.IsAny<User>()))
            .Returns("token");

        // Act
        await _controller.RegisterAsync(registerRequest);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser.FirstName.Should().Be(registerRequest.FirstName);
        capturedUser.LastName.Should().Be(registerRequest.LastName);
        capturedUser.Email.Should().Be(registerRequest.Email);
        capturedUser.Password.Should().Be(registerRequest.Password);
        capturedUser.PhoneNumber.Should().Be(registerRequest.PhoneNumber);
        capturedUser.Role.Should().Be(UserRole.Free);
        capturedUser.Status.Should().Be(UserStatus.Active);
        capturedUser.IsEmailVerified.Should().BeFalse();
    }

    #endregion

    #region MapUserToDto Tests

    [Fact]
    public async Task LoginAsync_ShouldMapUserToDtoCorrectly()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123"
        };
        var user = CreateTestUser();
        user.Bio = "Test bio";
        user.ProfilePictureUrl = "https://example.com/avatar.jpg";
        user.PhoneNumber = "+1234567890";
        user.IsEmailVerified = true;
        user.LastLoginAt = DateTime.UtcNow.AddDays(-1);

        _mockUserService.Setup(x => x.Authenticate(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync(user);
        _mockJwtService.Setup(x => x.GenerateJwtToken(user))
            .Returns("token");

        // Act
        var result = await _controller.LoginAsync(loginRequest);

        // Assert
        result.Data.Should().NotBeNull();
        var userDto = result.Data!.User;
        userDto.Id.Should().Be(user.Id);
        userDto.FirstName.Should().Be(user.FirstName);
        userDto.LastName.Should().Be(user.LastName);
        userDto.Email.Should().Be(user.Email);
        userDto.PhoneNumber.Should().Be(user.PhoneNumber);
        userDto.Bio.Should().Be(user.Bio);
        userDto.ProfilePictureUrl.Should().Be(user.ProfilePictureUrl);
        userDto.Role.Should().Be(user.Role.ToString());
        userDto.Status.Should().Be(user.Status.ToString());
        userDto.IsEmailVerified.Should().Be(user.IsEmailVerified);
        userDto.LastLoginAt.Should().Be(user.LastLoginAt);
        userDto.CreatedAt.Should().Be(user.CreatedAt);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task AuthenticationWorkflow_LoginAfterRegistration_ShouldWork()
    {
        // This test simulates the flow of registering a user and then logging in
        // Note: In a real scenario, this would be separate requests, but we're testing the controller logic

        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        var loginRequest = new LoginRequestDto
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        var registeredUser = CreateTestUser();
        registeredUser.Email = registerRequest.Email;

        // Setup for registration
        _mockUserService.Setup(x => x.GetUserByEmail(registerRequest.Email))
            .ReturnsAsync((User?)null);
        _mockUserService.Setup(x => x.Register(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _mockJwtService.Setup(x => x.GenerateJwtToken(It.IsAny<User>()))
            .Returns("register-token");

        // Setup for login
        _mockUserService.Setup(x => x.Authenticate(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync(registeredUser);

        // Act
        var registerResult = await _controller.RegisterAsync(registerRequest);
        var loginResult = await _controller.LoginAsync(loginRequest);

        // Assert
        registerResult.IsSuccess.Should().BeTrue();
        registerResult.Data!.Token.Should().Be("register-token");

        loginResult.IsSuccess.Should().BeTrue();
        loginResult.Data!.User.Email.Should().Be(registerRequest.Email);
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
            PasswordHash = "hashedpassword",
            PasswordSalt = new byte[128],
            Role = UserRole.Free,
            Status = UserStatus.Active,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private void VerifyInformationLog(string messageTemplate)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private void VerifyWarningLog(string messageTemplate)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private void VerifyErrorLog(string messageTemplate)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}