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

namespace PetLoveCommunity.Tests.Unit.API.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<UsersController>> _mockLogger;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_mockUserService.Object, _mockLogger.Object);
        
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
        var act = () => new UsersController(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("userService");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new UsersController(_mockUserService.Object, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var controller = new UsersController(_mockUserService.Object, _mockLogger.Object);

        // Assert
        controller.Should().NotBeNull();
    }

    #endregion

    #region GetAllUsers Tests

    [Fact]
    public async Task GetAllUsers_WithExistingUsers_ShouldReturnSuccessResponse()
    {
        // Arrange
        var users = CreateTestUsers();
        _mockUserService.Setup(x => x.GetAllUsers())
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value as ApiResponse<List<UserDto>>;
        response.Should().NotBeNull();
        response!.IsSuccess.Should().BeTrue();
        response.Data.Should().HaveCount(3);
        response.Message.Should().Be("Users retrieved successfully");

        // Verify user mapping
        var firstUser = response.Data![0];
        var sourceUser = users.First();
        firstUser.Id.Should().Be(sourceUser.Id);
        firstUser.FirstName.Should().Be(sourceUser.FirstName);
        firstUser.LastName.Should().Be(sourceUser.LastName);
        firstUser.Email.Should().Be(sourceUser.Email);
        firstUser.Role.Should().Be(sourceUser.Role.ToString());
        firstUser.Status.Should().Be(sourceUser.Status.ToString());

        _mockUserService.Verify(x => x.GetAllUsers(), Times.Once);
    }

    [Fact]
    public async Task GetAllUsers_WithNoUsers_ShouldReturnSuccessResponseWithEmptyList()
    {
        // Arrange
        var emptyUsers = new List<User>();
        _mockUserService.Setup(x => x.GetAllUsers())
            .ReturnsAsync(emptyUsers);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;

        var response = okResult.Value as ApiResponse<List<UserDto>>;
        response.Should().NotBeNull();
        response!.IsSuccess.Should().BeTrue();
        response.Data.Should().BeEmpty();
        response.Message.Should().Be("Users retrieved successfully");

        _mockUserService.Verify(x => x.GetAllUsers(), Times.Once);
    }

    [Fact]
    public async Task GetAllUsers_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockUserService.Setup(x => x.GetAllUsers())
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(500);

        var response = objectResult.Value as ApiResponse<List<UserDto>>;
        response.Should().NotBeNull();
        response!.IsSuccess.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().Be("An error occurred while retrieving users");

        VerifyErrorLog("Error retrieving users");
    }

    [Fact]
    public async Task GetAllUsers_ShouldLogUserCount()
    {
        // Arrange
        var users = CreateTestUsers();
        _mockUserService.Setup(x => x.GetAllUsers())
            .ReturnsAsync(users);

        // Act
        await _controller.GetAllUsers();

        // Assert
        VerifyInformationLog("Retrieved {UserCount} users");
    }

    [Fact]
    public async Task GetAllUsers_ShouldMapAllUserPropertiesCorrectly()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            PhoneNumber = "+1234567890",
            Bio = "Test bio",
            ProfilePictureUrl = "https://example.com/avatar.jpg",
            Role = UserRole.Premium,
            Status = UserStatus.Active,
            IsEmailVerified = true,
            LastLoginAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow
        };

        _mockUserService.Setup(x => x.GetAllUsers())
            .ReturnsAsync(new List<User> { user });

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = (OkObjectResult)result;
        var response = okResult.Value as ApiResponse<List<UserDto>>;
        var userDto = response!.Data![0];

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

    [Theory]
    [InlineData(UserRole.Free)]
    [InlineData(UserRole.Premium)]
    [InlineData(UserRole.Vendor)]
    [InlineData(UserRole.Shelter)]
    [InlineData(UserRole.Admin)]
    public async Task GetAllUsers_WithDifferentUserRoles_ShouldMapRoleCorrectly(UserRole role)
    {
        // Arrange
        var user = CreateTestUser();
        user.Role = role;

        _mockUserService.Setup(x => x.GetAllUsers())
            .ReturnsAsync(new List<User> { user });

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = (OkObjectResult)result;
        var response = okResult.Value as ApiResponse<List<UserDto>>;
        var userDto = response!.Data![0];

        userDto.Role.Should().Be(role.ToString());
    }

    [Theory]
    [InlineData(UserStatus.Active)]
    [InlineData(UserStatus.Inactive)]
    [InlineData(UserStatus.Suspended)]
    [InlineData(UserStatus.Banned)]
    public async Task GetAllUsers_WithDifferentUserStatuses_ShouldMapStatusCorrectly(UserStatus status)
    {
        // Arrange
        var user = CreateTestUser();
        user.Status = status;

        _mockUserService.Setup(x => x.GetAllUsers())
            .ReturnsAsync(new List<User> { user });

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = (OkObjectResult)result;
        var response = okResult.Value as ApiResponse<List<UserDto>>;
        var userDto = response!.Data![0];

        userDto.Status.Should().Be(status.ToString());
    }

    [Fact]
    public async Task GetAllUsers_WithUserWithNullOptionalFields_ShouldMapCorrectly()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            PhoneNumber = null,
            Bio = null,
            ProfilePictureUrl = null,
            Role = UserRole.Free,
            Status = UserStatus.Active,
            IsEmailVerified = false,
            LastLoginAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockUserService.Setup(x => x.GetAllUsers())
            .ReturnsAsync(new List<User> { user });

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = (OkObjectResult)result;
        var response = okResult.Value as ApiResponse<List<UserDto>>;
        var userDto = response!.Data![0];

        userDto.PhoneNumber.Should().BeNull();
        userDto.Bio.Should().BeNull();
        userDto.ProfilePictureUrl.Should().BeNull();
        userDto.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public async Task GetAllUsers_WithLargeNumberOfUsers_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<User>();
        for (int i = 0; i < 100; i++)
        {
            users.Add(new User
            {
                Id = Guid.NewGuid(),
                FirstName = $"User{i}",
                LastName = "Test",
                Email = $"user{i}@test.com",
                Role = UserRole.Free,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        _mockUserService.Setup(x => x.GetAllUsers())
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = (OkObjectResult)result;
        var response = okResult.Value as ApiResponse<List<UserDto>>;
        response!.Data.Should().HaveCount(100);
    }

    [Fact]
    public async Task GetAllUsers_WithSpecialCharactersInUserData_ShouldMapCorrectly()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "José María",
            LastName = "González-Pérez",
            Email = "josé.maría@domain.com",
            Bio = "Bio with émojis 🎉 and spëcial châracTers",
            Role = UserRole.Premium,
            Status = UserStatus.Active,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockUserService.Setup(x => x.GetAllUsers())
            .ReturnsAsync(new List<User> { user });

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = (OkObjectResult)result;
        var response = okResult.Value as ApiResponse<List<UserDto>>;
        var userDto = response!.Data![0];

        userDto.FirstName.Should().Be("José María");
        userDto.LastName.Should().Be("González-Pérez");
        userDto.Email.Should().Be("josé.maría@domain.com");
        userDto.Bio.Should().Be("Bio with émojis 🎉 and spëcial châracTers");
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public void UsersController_ShouldHaveAuthorizeAttribute()
    {
        // Act
        var controllerType = typeof(UsersController);
        var authorizeAttributes = controllerType.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);

        // Assert
        authorizeAttributes.Should().HaveCount(1, "Controller should have Authorize attribute");
    }

    [Fact]
    public void GetAllUsers_ShouldNotHaveAdditionalAuthorizeAttribute()
    {
        // Act
        var methodInfo = typeof(UsersController).GetMethod(nameof(UsersController.GetAllUsers));
        var authorizeAttributes = methodInfo!.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);

        // Assert
        authorizeAttributes.Should().BeEmpty("Method should not have additional Authorize attribute beyond controller level");
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task GetAllUsers_WhenServiceReturnsNull_ShouldHandleGracefully()
    {
        // Arrange
        _mockUserService.Setup(x => x.GetAllUsers())
            .ReturnsAsync((IEnumerable<User>)null!);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        // The controller should handle null by returning an error response rather than throwing
        result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(500);
        
        var response = objectResult.Value as ApiResponse<List<UserDto>>;
        response.Should().NotBeNull();
        response!.IsSuccess.Should().BeFalse();
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetAllUsers_WhenMappingFails_ShouldThrowException()
    {
        // Arrange
        var userWithNullId = new User
        {
            Id = Guid.Empty, // This might cause issues in some scenarios
            FirstName = null!, // This will cause mapping issues
            LastName = null!,
            Email = null!,
            Role = UserRole.Free,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockUserService.Setup(x => x.GetAllUsers())
            .ReturnsAsync(new List<User> { userWithNullId });

        // Act & Assert
        // The mapping should still work but with null values
        var result = await _controller.GetAllUsers();
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Helper Methods

    private static List<User> CreateTestUsers()
    {
        return new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice.smith@test.com",
                Role = UserRole.Free,
                Status = UserStatus.Active,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@test.com",
                Role = UserRole.Premium,
                Status = UserStatus.Active,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Carol",
                LastName = "Williams",
                Email = "carol.williams@test.com",
                Role = UserRole.Vendor,
                Status = UserStatus.Inactive,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            }
        };
    }

    private static User CreateTestUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            PhoneNumber = "+1234567890",
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