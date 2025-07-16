using FluentAssertions;
using PetLoveCommunity.Application.Services;

namespace PetLoveCommunity.Tests.Unit.Application.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    #region CreatePasswordHash Tests

    [Fact]
    public void CreatePasswordHash_WithValidPassword_ShouldGenerateHashAndSalt()
    {
        // Arrange
        const string password = "TestPassword123!";

        // Act
        _passwordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Assert
        hash.Should().NotBeNull();
        salt.Should().NotBeNull();
        hash.Length.Should().Be(64, "HMACSHA512 produces 64-byte hashes");
        salt.Length.Should().Be(128, "HMACSHA512 uses 128-byte keys");
        hash.Should().NotBeEquivalentTo(new byte[64], "hash should not be empty");
        salt.Should().NotBeEquivalentTo(new byte[128], "salt should not be empty");
    }

    [Fact]
    public void CreatePasswordHash_WithSamePassword_ShouldGenerateDifferentHashesAndSalts()
    {
        // Arrange
        const string password = "TestPassword123!";

        // Act
        _passwordHasher.CreatePasswordHash(password, out byte[] hash1, out byte[] salt1);
        _passwordHasher.CreatePasswordHash(password, out byte[] hash2, out byte[] salt2);

        // Assert
        hash1.Should().NotBeEquivalentTo(hash2, "same password should generate different hashes due to different salts");
        salt1.Should().NotBeEquivalentTo(salt2, "salts should be random and different each time");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void CreatePasswordHash_WithEmptyOrWhitespacePassword_ShouldThrowArgumentException(string password)
    {
        // Act & Assert
        var act = () => _passwordHasher.CreatePasswordHash(password, out _, out _);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password cannot be empty or whitespace only string. (Parameter 'password')");
    }

    [Fact]
    public void CreatePasswordHash_WithNullPassword_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act = () => _passwordHasher.CreatePasswordHash(null!, out _, out _);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password cannot be empty or whitespace only string. (Parameter 'password')");
    }

    [Theory]
    [InlineData("a")]
    [InlineData("short")]
    [InlineData("VeryLongPasswordThatShouldStillWorkFineRegardlessOfLength123456789!@#$%^&*()")]
    public void CreatePasswordHash_WithDifferentPasswordLengths_ShouldWork(string password)
    {
        // Act
        _passwordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Assert
        hash.Should().NotBeNull();
        salt.Should().NotBeNull();
        hash.Length.Should().Be(64);
        salt.Length.Should().Be(128);
    }

    [Theory]
    [InlineData("password")]
    [InlineData("P@ssw0rd!")]
    [InlineData("密码")]
    [InlineData("пароль")]
    [InlineData("🔒password🔑")]
    public void CreatePasswordHash_WithSpecialCharactersAndUnicode_ShouldWork(string password)
    {
        // Act
        _passwordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Assert
        hash.Should().NotBeNull();
        salt.Should().NotBeNull();
        hash.Length.Should().Be(64);
        salt.Length.Should().Be(128);
    }

    #endregion

    #region VerifyPasswordHash Tests

    [Fact]
    public void VerifyPasswordHash_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        const string password = "TestPassword123!";
        _passwordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Act
        var result = _passwordHasher.VerifyPasswordHash(password, hash, salt);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPasswordHash_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        const string correctPassword = "TestPassword123!";
        const string incorrectPassword = "WrongPassword123!";
        _passwordHasher.CreatePasswordHash(correctPassword, out byte[] hash, out byte[] salt);

        // Act
        var result = _passwordHasher.VerifyPasswordHash(incorrectPassword, hash, salt);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void VerifyPasswordHash_WithEmptyOrWhitespacePassword_ShouldThrowArgumentException(string password)
    {
        // Arrange
        _passwordHasher.CreatePasswordHash("validpassword", out byte[] hash, out byte[] salt);

        // Act & Assert
        var act = () => _passwordHasher.VerifyPasswordHash(password, hash, salt);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password cannot be empty or whitespace only string. (Parameter 'password')");
    }

    [Fact]
    public void VerifyPasswordHash_WithNullPassword_ShouldThrowArgumentException()
    {
        // Arrange
        _passwordHasher.CreatePasswordHash("validpassword", out byte[] hash, out byte[] salt);

        // Act & Assert
        var act = () => _passwordHasher.VerifyPasswordHash(null!, hash, salt);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password cannot be empty or whitespace only string. (Parameter 'password')");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    [InlineData(63)]
    [InlineData(65)]
    [InlineData(128)]
    public void VerifyPasswordHash_WithInvalidHashLength_ShouldThrowArgumentException(int hashLength)
    {
        // Arrange
        const string password = "TestPassword123!";
        var invalidHash = new byte[hashLength];
        var validSalt = new byte[128];

        // Act & Assert
        var act = () => _passwordHasher.VerifyPasswordHash(password, invalidHash, validSalt);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid length of password hash (64 bytes expected). (Parameter 'passwordHash')");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(64)]
    [InlineData(127)]
    [InlineData(129)]
    [InlineData(256)]
    public void VerifyPasswordHash_WithInvalidSaltLength_ShouldThrowArgumentException(int saltLength)
    {
        // Arrange
        const string password = "TestPassword123!";
        var validHash = new byte[64];
        var invalidSalt = new byte[saltLength];

        // Act & Assert
        var act = () => _passwordHasher.VerifyPasswordHash(password, validHash, invalidSalt);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid length of password salt (128 bytes expected). (Parameter 'passwordSalt')");
    }

    [Fact]
    public void VerifyPasswordHash_WithCorruptedHash_ShouldReturnFalse()
    {
        // Arrange
        const string password = "TestPassword123!";
        _passwordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);
        
        // Corrupt the hash
        hash[0] = (byte)(hash[0] == 0 ? 1 : 0);

        // Act
        var result = _passwordHasher.VerifyPasswordHash(password, hash, salt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPasswordHash_WithCorruptedSalt_ShouldReturnFalse()
    {
        // Arrange
        const string password = "TestPassword123!";
        _passwordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);
        
        // Corrupt the salt
        salt[0] = (byte)(salt[0] == 0 ? 1 : 0);

        // Act
        var result = _passwordHasher.VerifyPasswordHash(password, hash, salt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPasswordHash_CaseSensitive_ShouldReturnFalse()
    {
        // Arrange
        const string password = "TestPassword123!";
        const string wrongCasePassword = "testpassword123!";
        _passwordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Act
        var result = _passwordHasher.VerifyPasswordHash(wrongCasePassword, hash, salt);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("密码", "密码")]
    [InlineData("пароль", "пароль")]
    [InlineData("🔒password🔑", "🔒password🔑")]
    public void VerifyPasswordHash_WithUnicodePasswords_ShouldWork(string originalPassword, string verifyPassword)
    {
        // Arrange
        _passwordHasher.CreatePasswordHash(originalPassword, out byte[] hash, out byte[] salt);

        // Act
        var result = _passwordHasher.VerifyPasswordHash(verifyPassword, hash, salt);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void PasswordHashingWorkflow_CompleteFlow_ShouldWork()
    {
        // Arrange
        const string password = "MySecurePassword123!";

        // Act - Create hash
        _passwordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Act - Verify correct password
        var correctVerification = _passwordHasher.VerifyPasswordHash(password, hash, salt);
        
        // Act - Verify incorrect password
        var incorrectVerification = _passwordHasher.VerifyPasswordHash("WrongPassword", hash, salt);

        // Assert
        correctVerification.Should().BeTrue("correct password should verify successfully");
        incorrectVerification.Should().BeFalse("incorrect password should fail verification");
        hash.Length.Should().Be(64);
        salt.Length.Should().Be(128);
    }

    [Fact]
    public void PasswordHasher_MultipleInstances_ShouldProduceDifferentResults()
    {
        // Arrange
        const string password = "TestPassword123!";
        var hasher1 = new PasswordHasher();
        var hasher2 = new PasswordHasher();

        // Act
        hasher1.CreatePasswordHash(password, out byte[] hash1, out byte[] salt1);
        hasher2.CreatePasswordHash(password, out byte[] hash2, out byte[] salt2);

        // Assert
        hash1.Should().NotBeEquivalentTo(hash2, "different instances should produce different hashes");
        salt1.Should().NotBeEquivalentTo(salt2, "different instances should produce different salts");
        
        // But both should verify the same password correctly
        hasher1.VerifyPasswordHash(password, hash1, salt1).Should().BeTrue();
        hasher2.VerifyPasswordHash(password, hash2, salt2).Should().BeTrue();
        
        // Cross-verification should work too
        hasher1.VerifyPasswordHash(password, hash2, salt2).Should().BeTrue();
        hasher2.VerifyPasswordHash(password, hash1, salt1).Should().BeTrue();
    }

    #endregion
}