using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PetLoveCommunity.Application.Interfaces;
using PetLoveCommunity.Domain.Entities;
using PetLoveCommunity.Infrastructure.Data;
using PetLoveCommunity.Infrastructure.Services;

namespace PetLoveCommunity.Tests.Unit.Infrastructure.Services;

public class UserServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _userService = new UserService(_context, _mockPasswordHasher.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullContext_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new UserService(null!, _mockPasswordHasher.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");
    }

    [Fact]
    public void Constructor_WithNullPasswordHasher_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new UserService(_context, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("passwordHasher");
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var service = new UserService(_context, _mockPasswordHasher.Object);

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region Authenticate Tests

    [Fact]
    public async Task Authenticate_WithValidCredentials_ShouldReturnUserAndUpdateLastLogin()
    {
        // Arrange
        var user = await SeedTestUser();
        const string password = "testpassword";
        
        // Use the same hash/salt values that are stored in the seeded user
        var passwordHash = Convert.FromBase64String(user.PasswordHash);
        var passwordSalt = user.PasswordSalt;

        _mockPasswordHasher.Setup(x => x.VerifyPasswordHash(password, passwordHash, passwordSalt))
            .Returns(true);

        var beforeAuth = DateTime.UtcNow;

        // Act
        var result = await _userService.Authenticate(user.Email, password);

        // Assert
        var afterAuth = DateTime.UtcNow;
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.LastLoginAt.Should().BeAfter(beforeAuth).And.BeBefore(afterAuth.AddSeconds(1));

        // Verify the user was updated in the database
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.LastLoginAt.Should().BeAfter(beforeAuth);
    }

    [Fact]
    public async Task Authenticate_WithInvalidEmail_ShouldReturnNull()
    {
        // Arrange
        await SeedTestUser();
        const string invalidEmail = "nonexistent@test.com";
        const string password = "testpassword";

        // Act
        var result = await _userService.Authenticate(invalidEmail, password);

        // Assert
        result.Should().BeNull();
        _mockPasswordHasher.Verify(x => x.VerifyPasswordHash(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task Authenticate_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        var user = await SeedTestUser();
        const string password = "wrongpassword";
        
        // Use the same hash/salt values that are stored in the seeded user
        var passwordHash = Convert.FromBase64String(user.PasswordHash);
        var passwordSalt = user.PasswordSalt;

        _mockPasswordHasher.Setup(x => x.VerifyPasswordHash(password, passwordHash, passwordSalt))
            .Returns(false);

        // Act
        var result = await _userService.Authenticate(user.Email, password);

        // Assert
        result.Should().BeNull();
        _mockPasswordHasher.Verify(x => x.VerifyPasswordHash(password, passwordHash, passwordSalt), Times.Once);
    }

    [Theory]
    [InlineData(null, "password")]
    [InlineData("", "password")]
    [InlineData("   ", "password")]
    [InlineData("user@test.com", null)]
    [InlineData("user@test.com", "")]
    [InlineData("user@test.com", "   ")]
    public async Task Authenticate_WithEmptyOrNullCredentials_ShouldReturnNull(string? email, string? password)
    {
        // Act
        var result = await _userService.Authenticate(email!, password!);

        // Assert
        result.Should().BeNull();
        _mockPasswordHasher.Verify(x => x.VerifyPasswordHash(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task Authenticate_WithUserNotFound_ShouldReturnNull()
    {
        // Arrange
        const string email = "nonexistent@test.com";
        const string password = "testpassword";

        // Act
        var result = await _userService.Authenticate(email, password);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task Register_WithValidUser_ShouldCreateUserWithHashedPassword()
    {
        // Arrange
        var user = CreateTestUserForRegistration();
        var passwordHash = new byte[64];
        var passwordSalt = new byte[128];

        _mockPasswordHasher.Setup(x => x.CreatePasswordHash(It.IsAny<string>(), out It.Ref<byte[]>.IsAny, out It.Ref<byte[]>.IsAny))
            .Callback((string password, out byte[] hash, out byte[] salt) =>
            {
                hash = passwordHash;
                salt = passwordSalt;
            });

        // Act
        await _userService.Register(user);

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        savedUser.Should().NotBeNull();
        savedUser!.FirstName.Should().Be(user.FirstName);
        savedUser.LastName.Should().Be(user.LastName);
        savedUser.Email.Should().Be(user.Email);
        savedUser.Role.Should().Be(UserRole.Free);
        savedUser.Status.Should().Be(UserStatus.Active);
        savedUser.IsEmailVerified.Should().BeFalse();
        savedUser.Password.Should().Be("****");
        savedUser.PasswordHash.Should().NotBeEmpty();

        _mockPasswordHasher.Verify(x => x.CreatePasswordHash(It.IsAny<string>(), out It.Ref<byte[]>.IsAny, out It.Ref<byte[]>.IsAny), Times.Once);
    }

    [Fact]
    public async Task Register_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = async () => await _userService.Register(null!);
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("user");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Register_WithEmptyPassword_ShouldThrowArgumentException(string? password)
    {
        // Arrange
        var user = CreateTestUserForRegistration();
        user.Password = password!;

        // Act & Assert
        var act = async () => await _userService.Register(user);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Password is required (Parameter 'user')");
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var existingUser = await SeedTestUser();
        var newUser = CreateTestUserForRegistration();
        newUser.Email = existingUser.Email;

        // Act & Assert
        var act = async () => await _userService.Register(newUser);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User with this email already exists");
    }

    [Fact]
    public async Task Register_ShouldSetDefaultValues()
    {
        // Arrange
        var user = CreateTestUserForRegistration();
        user.Role = UserRole.Admin; // Should be overridden
        user.Status = UserStatus.Banned; // Should be overridden
        user.IsEmailVerified = true; // Should be overridden

        var passwordHash = new byte[64];
        var passwordSalt = new byte[128];
        _mockPasswordHasher.Setup(x => x.CreatePasswordHash(It.IsAny<string>(), out It.Ref<byte[]>.IsAny, out It.Ref<byte[]>.IsAny))
            .Callback((string password, out byte[] hash, out byte[] salt) =>
            {
                hash = passwordHash;
                salt = passwordSalt;
            });

        // Act
        await _userService.Register(user);

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        savedUser!.Role.Should().Be(UserRole.Free);
        savedUser.Status.Should().Be(UserStatus.Active);
        savedUser.IsEmailVerified.Should().BeFalse();
    }

    #endregion

    #region GetAllUsers Tests

    [Fact]
    public async Task GetAllUsers_WithExistingUsers_ShouldReturnFilteredAndOrderedUsers()
    {
        // Arrange
        var user1 = CreateTestUser("Zoe", "Adams", "zoe@test.com");
        var user2 = CreateTestUser("Alice", "Brown", "alice@test.com");
        var user3 = CreateTestUser("Bob", "Adams", "bob@test.com");
        var deletedUser = CreateTestUser("Deleted", "User", "deleted@test.com");
        deletedUser.IsDeleted = true;

        _context.Users.AddRange(user1, user2, user3, deletedUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userService.GetAllUsers();

        // Assert
        var users = result.ToList();
        users.Should().HaveCount(3);
        users.Should().NotContain(u => u.IsDeleted);
        
        // Verify ordering: FirstName, then LastName
        users[0].FirstName.Should().Be("Alice");
        users[1].FirstName.Should().Be("Bob");
        users[2].FirstName.Should().Be("Zoe");
    }

    [Fact]
    public async Task GetAllUsers_WithNoUsers_ShouldReturnEmptyCollection()
    {
        // Act
        var result = await _userService.GetAllUsers();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllUsers_WithOnlyDeletedUsers_ShouldReturnEmptyCollection()
    {
        // Arrange
        var deletedUser1 = CreateTestUser("User", "One", "user1@test.com");
        var deletedUser2 = CreateTestUser("User", "Two", "user2@test.com");
        deletedUser1.IsDeleted = true;
        deletedUser2.IsDeleted = true;

        _context.Users.AddRange(deletedUser1, deletedUser2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userService.GetAllUsers();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetUserByEmail Tests

    [Fact]
    public async Task GetUserByEmail_WithValidEmail_ShouldReturnUser()
    {
        // Arrange
        var user = await SeedTestUser();

        // Act
        var result = await _userService.GetUserByEmail(user.Email);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetUserByEmail_WithNonExistentEmail_ShouldReturnNull()
    {
        // Arrange
        await SeedTestUser();
        const string nonExistentEmail = "nonexistent@test.com";

        // Act
        var result = await _userService.GetUserByEmail(nonExistentEmail);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetUserByEmail_WithEmptyEmail_ShouldReturnNull(string? email)
    {
        // Act
        var result = await _userService.GetUserByEmail(email!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByEmail_ShouldNotReturnDeletedUser()
    {
        // Arrange
        var user = CreateTestUser("Deleted", "User", "deleted@test.com");
        user.IsDeleted = true;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userService.GetUserByEmail(user.Email);

        // Assert
        // Global query filter should exclude deleted users
        result.Should().BeNull();
    }

    #endregion

    #region GetUserById Tests

    [Fact]
    public async Task GetUserById_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var user = await SeedTestUser();

        // Act
        var result = await _userService.GetUserById(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetUserById_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        await SeedTestUser();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _userService.GetUserById(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserById_WithEmptyGuid_ShouldReturnNull()
    {
        // Arrange
        await SeedTestUser();

        // Act
        var result = await _userService.GetUserById(Guid.Empty);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserById_ShouldNotReturnDeletedUser()
    {
        // Arrange
        var user = CreateTestUser("Deleted", "User", "deleted@test.com");
        user.IsDeleted = true;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userService.GetUserById(user.Id);

        // Assert
        // Global query filter should exclude deleted users
        result.Should().BeNull();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task UserServiceWorkflow_CompleteRegistrationAndAuth_ShouldWork()
    {
        // Arrange
        var user = CreateTestUserForRegistration();
        const string password = "TestPassword123!";
        user.Password = password;

        var passwordHash = new byte[64];
        var passwordSalt = new byte[128];

        _mockPasswordHasher.Setup(x => x.CreatePasswordHash(It.IsAny<string>(), out It.Ref<byte[]>.IsAny, out It.Ref<byte[]>.IsAny))
            .Callback((string password, out byte[] hash, out byte[] salt) =>
            {
                hash = passwordHash;
                salt = passwordSalt;
            });

        _mockPasswordHasher.Setup(x => x.VerifyPasswordHash(password, passwordHash, passwordSalt))
            .Returns(true);

        // Act - Register
        await _userService.Register(user);

        // Get the registered user to obtain the actual stored hash/salt
        var registeredUser = await _userService.GetUserByEmail(user.Email);
        
        // Update the mock for verification to use the stored hash/salt
        _mockPasswordHasher.Setup(x => x.VerifyPasswordHash(password, 
            Convert.FromBase64String(registeredUser!.PasswordHash), 
            registeredUser.PasswordSalt))
            .Returns(true);

        // Act - Authenticate
        var authenticatedUser = await _userService.Authenticate(user.Email, password);

        // Assert
        authenticatedUser.Should().NotBeNull();
        authenticatedUser!.Email.Should().Be(user.Email);
        authenticatedUser.FirstName.Should().Be(user.FirstName);
        authenticatedUser.LastName.Should().Be(user.LastName);
        authenticatedUser.LastLoginAt.Should().NotBeNull();

        _mockPasswordHasher.Verify(x => x.CreatePasswordHash(It.IsAny<string>(), out It.Ref<byte[]>.IsAny, out It.Ref<byte[]>.IsAny), Times.Once);
        _mockPasswordHasher.Verify(x => x.VerifyPasswordHash(password, It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Once);
    }

    #endregion

    #region Helper Methods

    private async Task<User> SeedTestUser()
    {
        var user = CreateTestUser("John", "Doe", "john.doe@test.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    private static User CreateTestUser(string firstName, string lastName, string email)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = Convert.ToBase64String(new byte[64]),
            PasswordSalt = new byte[128],
            Role = UserRole.Free,
            Status = UserStatus.Active,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static User CreateTestUserForRegistration()
    {
        return new User
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@test.com",
            Password = "TestPassword123!",
            PhoneNumber = "+1234567890"
        };
    }

    #endregion
}