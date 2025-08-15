using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PetLoveCommunity.Application.DTOs;
using PetLoveCommunity.Application.Interfaces;
using PetLoveCommunity.Application.Services;
using PetLoveCommunity.Domain.Entities;
using PetLoveCommunity.Domain.Interfaces;

namespace PetLoveCommunity.Tests.Unit.Application.Services;

public class PetServiceTests
{
    private readonly Mock<IPetRepository> _mockPetRepository;
    private readonly Mock<IAppConfig> _mockAppConfig;
    private readonly Mock<ILogger<PetService>> _mockLogger;
    private readonly PetService _petService;
    private const string BaseApiUrl = "https://api.example.com";

    public PetServiceTests()
    {
        _mockPetRepository = new Mock<IPetRepository>();
        _mockAppConfig = new Mock<IAppConfig>();
        _mockLogger = new Mock<ILogger<PetService>>();
        
        _mockAppConfig.Setup(x => x.BaseApiUrl).Returns(BaseApiUrl);
        
        _petService = new PetService(_mockPetRepository.Object, _mockAppConfig.Object, _mockLogger.Object);
    }

    #region GetAllAvailablePetsAsync Tests

    [Fact]
    public async Task GetAllAvailablePetsAsync_WhenPetsExist_ShouldReturnMappedPets()
    {
        // Arrange
        var pets = CreateTestPets();
        _mockPetRepository.Setup(x => x.GetAvailablePetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pets);

        // Act
        var result = await _petService.GetAllAvailablePetsAsync();

        // Assert
        result.Should().HaveCount(2);
        var resultArray = result.ToArray();
        resultArray[0].Name.Should().Be("Buddy");
        resultArray[0].PetType.Should().Be("Dog");
        resultArray[1].Name.Should().Be("Whiskers");
        resultArray[1].PetType.Should().Be("Cat");

        // Verify logging
        VerifyInformationLog("Retrieved {Count} available pets");
    }

    [Fact]
    public async Task GetAllAvailablePetsAsync_WhenNoPetsExist_ShouldReturnEmptyCollection()
    {
        // Arrange
        var emptyPets = new List<Pet>();
        _mockPetRepository.Setup(x => x.GetAvailablePetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPets);

        // Act
        var result = await _petService.GetAllAvailablePetsAsync();

        // Assert
        result.Should().BeEmpty();
        VerifyInformationLog("Retrieved {Count} available pets");
    }

    [Fact]
    public async Task GetAllAvailablePetsAsync_WhenRepositoryThrowsException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var exception = new Exception("Database connection failed");
        _mockPetRepository.Setup(x => x.GetAvailablePetsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var act = async () => await _petService.GetAllAvailablePetsAsync();
        await act.Should().ThrowAsync<Exception>().WithMessage("Database connection failed");

        VerifyErrorLog("Error occurred while retrieving available pets");
    }

    #endregion

    #region GetPetByIdAsync Tests

    [Fact]
    public async Task GetPetByIdAsync_WhenPetExists_ShouldReturnMappedPet()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var pet = CreateTestPet(petId, "Buddy", PetType.Dog);
        _mockPetRepository.Setup(x => x.GetWithPhotosAsync(petId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pet);

        // Act
        var result = await _petService.GetPetByIdAsync(petId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(petId);
        result.Name.Should().Be("Buddy");
        result.PetType.Should().Be("Dog");

        VerifyInformationLog("Retrieved pet details for ID {PetId}");
    }

    [Fact]
    public async Task GetPetByIdAsync_WhenPetDoesNotExist_ShouldReturnNullAndLogWarning()
    {
        // Arrange
        var petId = Guid.NewGuid();
        _mockPetRepository.Setup(x => x.GetWithPhotosAsync(petId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pet?)null);

        // Act
        var result = await _petService.GetPetByIdAsync(petId);

        // Assert
        result.Should().BeNull();
        VerifyWarningLog("Pet with ID {PetId} not found");
    }

    [Fact]
    public async Task GetPetByIdAsync_WhenRepositoryThrowsException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var exception = new Exception("Database error");
        _mockPetRepository.Setup(x => x.GetWithPhotosAsync(petId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var act = async () => await _petService.GetPetByIdAsync(petId);
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");

        VerifyErrorLog("Error occurred while retrieving pet with ID {PetId}");
    }

    #endregion

    #region GetPetsByTypeAsync Tests

    [Theory]
    [InlineData("Dog", PetType.Dog)]
    [InlineData("cat", PetType.Cat)]
    [InlineData("BIRD", PetType.Bird)]
    [InlineData("fish", PetType.Fish)]
    public async Task GetPetsByTypeAsync_WithValidPetType_ShouldReturnFilteredPets(string petTypeString, PetType expectedType)
    {
        // Arrange
        var pets = new List<Pet> { CreateTestPet(Guid.NewGuid(), "Test Pet", expectedType) };
        _mockPetRepository.Setup(x => x.GetPetsByTypeAsync(expectedType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pets);

        // Act
        var result = await _petService.GetPetsByTypeAsync(petTypeString);

        // Assert
        result.Should().HaveCount(1);
        result.First().PetType.Should().Be(expectedType.ToString());
        VerifyInformationLog("Retrieved {Count} pets of type {PetType}");
    }

    [Theory]
    [InlineData("InvalidType")]
    [InlineData("NotAnEnum")]
    [InlineData("SomethingElse")]
    public async Task GetPetsByTypeAsync_WithInvalidPetType_ShouldReturnEmptyAndLogWarning(string invalidPetType)
    {
        // Act
        var result = await _petService.GetPetsByTypeAsync(invalidPetType);

        // Assert
        result.Should().BeEmpty();
        VerifyWarningLog("Invalid pet type: {PetType}");
        
        // Verify repository was not called
        _mockPetRepository.Verify(x => x.GetPetsByTypeAsync(It.IsAny<PetType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetPetsByTypeAsync_WhenRepositoryThrowsException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        const string petType = "Dog";
        var exception = new Exception("Database error");
        _mockPetRepository.Setup(x => x.GetPetsByTypeAsync(PetType.Dog, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var act = async () => await _petService.GetPetsByTypeAsync(petType);
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");

        VerifyErrorLog("Error occurred while retrieving pets of type {PetType}");
    }

    #endregion

    #region SearchPetsAsync Tests

    [Fact]
    public async Task SearchPetsAsync_WithValidSearchTermAndType_ShouldReturnFilteredPets()
    {
        // Arrange
        const string searchTerm = "friendly";
        const string petType = "Dog";
        var pets = CreateTestPets();
        _mockPetRepository.Setup(x => x.SearchPetsAsync(searchTerm, PetType.Dog, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pets);

        // Act
        var result = await _petService.SearchPetsAsync(searchTerm, petType);

        // Assert
        result.Should().HaveCount(2);
        VerifyInformationLog("Search for '{SearchTerm}' returned {Count} pets");
    }

    [Fact]
    public async Task SearchPetsAsync_WithSearchTermOnly_ShouldReturnResults()
    {
        // Arrange
        const string searchTerm = "friendly";
        var pets = CreateTestPets();
        _mockPetRepository.Setup(x => x.SearchPetsAsync(searchTerm, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pets);

        // Act
        var result = await _petService.SearchPetsAsync(searchTerm);

        // Assert
        result.Should().HaveCount(2);
        VerifyInformationLog("Search for '{SearchTerm}' returned {Count} pets");
    }

    [Fact]
    public async Task SearchPetsAsync_WithInvalidPetType_ShouldIgnoreTypeAndSearchWithNull()
    {
        // Arrange
        const string searchTerm = "friendly";
        const string invalidPetType = "InvalidType";
        var pets = CreateTestPets();
        _mockPetRepository.Setup(x => x.SearchPetsAsync(searchTerm, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pets);

        // Act
        var result = await _petService.SearchPetsAsync(searchTerm, invalidPetType);

        // Assert
        result.Should().HaveCount(2);
        _mockPetRepository.Verify(x => x.SearchPetsAsync(searchTerm, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchPetsAsync_WhenRepositoryThrowsException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        const string searchTerm = "test";
        var exception = new Exception("Search failed");
        _mockPetRepository.Setup(x => x.SearchPetsAsync(searchTerm, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var act = async () => await _petService.SearchPetsAsync(searchTerm);
        await act.Should().ThrowAsync<Exception>().WithMessage("Search failed");

        VerifyErrorLog("Error occurred while searching pets with term '{SearchTerm}'");
    }

    #endregion

    #region GetPetsByOwnerAsync Tests

    [Fact]
    public async Task GetPetsByOwnerAsync_WithValidOwnerId_ShouldReturnOwnerPets()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var pets = CreateTestPets();
        _mockPetRepository.Setup(x => x.GetPetsByOwnerAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pets);

        // Act
        var result = await _petService.GetPetsByOwnerAsync(ownerId);

        // Assert
        result.Should().HaveCount(2);
        VerifyInformationLog("Retrieved {Count} pets for owner {OwnerId}");
    }

    [Fact]
    public async Task GetPetsByOwnerAsync_WhenRepositoryThrowsException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var exception = new Exception("Database error");
        _mockPetRepository.Setup(x => x.GetPetsByOwnerAsync(ownerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var act = async () => await _petService.GetPetsByOwnerAsync(ownerId);
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");

        VerifyErrorLog("Error occurred while retrieving pets for owner {OwnerId}");
    }

    #endregion

    #region IncrementViewsAsync Tests

    [Fact]
    public async Task IncrementViewsAsync_WhenPetExists_ShouldIncrementViewsAndUpdate()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var pet = CreateTestPet(petId, "Buddy", PetType.Dog);
        pet.Views = 5;

        _mockPetRepository.Setup(x => x.GetByIdAsync(petId))
            .ReturnsAsync(pet);
        _mockPetRepository.Setup(x => x.UpdateAsync(pet))
            .ReturnsAsync(pet);

        // Act
        await _petService.IncrementViewsAsync(petId);

        // Assert
        pet.Views.Should().Be(6);
        _mockPetRepository.Verify(x => x.UpdateAsync(pet), Times.Once);
        VerifyDebugLog("Incremented views for pet {PetId} to {Views}");
    }

    [Fact]
    public async Task IncrementViewsAsync_WhenPetDoesNotExist_ShouldNotUpdate()
    {
        // Arrange
        var petId = Guid.NewGuid();
        _mockPetRepository.Setup(x => x.GetByIdAsync(petId))
            .ReturnsAsync((Pet?)null);

        // Act
        await _petService.IncrementViewsAsync(petId);

        // Assert
        _mockPetRepository.Verify(x => x.UpdateAsync(It.IsAny<Pet>()), Times.Never);
    }

    [Fact]
    public async Task IncrementViewsAsync_WhenRepositoryThrowsException_ShouldLogErrorButNotThrow()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var exception = new Exception("Database error");
        _mockPetRepository.Setup(x => x.GetByIdAsync(petId))
            .ThrowsAsync(exception);

        // Act
        var act = async () => await _petService.IncrementViewsAsync(petId);

        // Assert
        await act.Should().NotThrowAsync();
        VerifyErrorLog("Error occurred while incrementing views for pet {PetId}");
    }

    #endregion

    #region NotImplemented Methods Tests

    [Fact]
    public async Task CreatePetAsync_ShouldThrowNotImplementedException()
    {
        // Arrange
        var petDto = new PetDetailDto { Name = "Test Pet" };

        // Act & Assert
        var act = async () => await _petService.CreatePetAsync(petDto);
        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("CreatePetAsync not yet implemented");

        VerifyInformationLog("Creating new pet: {PetName}");
    }

    [Fact]
    public async Task UpdatePetAsync_ShouldThrowNotImplementedException()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var petDto = new PetDetailDto { Name = "Test Pet" };

        // Act & Assert
        var act = async () => await _petService.UpdatePetAsync(petId, petDto);
        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("UpdatePetAsync not yet implemented");

        VerifyInformationLog("Updating pet with ID {PetId}");
    }

    [Fact]
    public async Task DeletePetAsync_ShouldThrowNotImplementedException()
    {
        // Arrange
        var petId = Guid.NewGuid();

        // Act & Assert
        var act = async () => await _petService.DeletePetAsync(petId);
        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("DeletePetAsync not yet implemented");

        VerifyInformationLog("Deleting pet with ID {PetId}");
    }

    #endregion

    #region Helper Methods

    private static List<Pet> CreateTestPets()
    {
        return new List<Pet>
        {
            CreateTestPet(Guid.NewGuid(), "Buddy", PetType.Dog),
            CreateTestPet(Guid.NewGuid(), "Whiskers", PetType.Cat)
        };
    }

    private static Pet CreateTestPet(Guid id, string name, PetType petType)
    {
        return new Pet
        {
            Id = id,
            Name = name,
            PetType = petType,
            Breed = "Test Breed",
            Image = "test-image.jpg",
            DateOfBirth = DateTime.UtcNow.AddYears(-2),
            Price = 100.00m,
            Description = "A friendly pet",
            Gender = PetGender.Male,
            AdoptionStatus = AdoptionStatus.Available,
            IsActive = true,
            Views = 0,
            OwnerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Owner = new User { Id = Guid.NewGuid(), FirstName = "Test", LastName = "Owner", Email = "owner@test.com" },
            Photos = new List<PetPhoto>()
        };
    }

    private void VerifyInformationLog(string message)
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

    private void VerifyWarningLog(string message)
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

    private void VerifyErrorLog(string message)
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

    private void VerifyDebugLog(string message)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}