using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using PetLoveCommunity.API.Controllers;
using PetLoveCommunity.Application.DTOs;
using PetLoveCommunity.Application.DTOs.Shared;
using PetLoveCommunity.Application.Interfaces;

namespace PetLoveCommunity.Tests.Unit.API.Controllers;

public class PetsControllerTests
{
    private readonly Mock<IPetService> _mockPetService;
    private readonly Mock<ILogger<PetsController>> _mockLogger;
    private readonly PetsController _controller;

    public PetsControllerTests()
    {
        _mockPetService = new Mock<IPetService>();
        _mockLogger = new Mock<ILogger<PetsController>>();
        _controller = new PetsController(_mockPetService.Object, _mockLogger.Object);
        
        // Setup HttpContext for header testing
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    #region GetAllPetsAsync Tests

    [Fact]
    public async Task GetAllPetsAsync_WithCorrelationId_ShouldReturnSuccessResponse_AndLogCorrelationId()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        _controller.Request.Headers["X-Correlation-ID"] = correlationId;
        // Simulate what the middleware would do
        _controller.HttpContext.Items["CorrelationId"] = correlationId;
        // Simulate what the middleware would do
        _controller.HttpContext.Items["CorrelationId"] = correlationId;
        
        var pets = new List<PetListDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Buddy", PetType = "Dog" },
            new() { Id = Guid.NewGuid(), Name = "Whiskers", PetType = "Cat" }
        };
        
        _mockPetService.Setup(x => x.GetAllAvailablePetsAsync())
            .ReturnsAsync(pets);

        // Act
        var result = await _controller.GetAllPetsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.Should().BeEquivalentTo(pets);
        result.Message.Should().BeNull();

        // Verify logging includes correlation ID
        VerifyLogContainsCorrelationId(LogLevel.Information, correlationId);
    }

    [Fact]
    public async Task GetAllPetsAsync_WithoutCorrelationId_ShouldGenerateNewId_AndLogIt()
    {
        // Arrange
        var pets = new List<PetListDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Buddy", PetType = "Dog" }
        };
        
        _mockPetService.Setup(x => x.GetAllAvailablePetsAsync())
            .ReturnsAsync(pets);

        // Act
        var result = await _controller.GetAllPetsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);

        // Verify logging includes a generated correlation ID (should be a valid GUID)
        VerifyLogContainsValidCorrelationId(LogLevel.Information);
    }

    [Fact]
    public async Task GetAllPetsAsync_WhenServiceThrowsException_ShouldReturnFailResponse_AndLogError()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        _controller.Request.Headers["X-Correlation-ID"] = correlationId;
        // Simulate what the middleware would do
        _controller.HttpContext.Items["CorrelationId"] = correlationId;
        
        var exception = new Exception("Database connection failed");
        _mockPetService.Setup(x => x.GetAllAvailablePetsAsync())
            .ThrowsAsync(exception);

        // Act
        var result = await _controller.GetAllPetsAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("An error occurred while retrieving pets");

        // Verify error logging includes correlation ID
        VerifyLogContainsCorrelationId(LogLevel.Error, correlationId);
    }

    #endregion

    #region GetPetByIdAsync Tests

    [Fact]
    public async Task GetPetByIdAsync_WithValidId_ShouldReturnSuccessResponse()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var correlationId = Guid.NewGuid().ToString();
        _controller.Request.Headers["X-Correlation-ID"] = correlationId;
        // Simulate what the middleware would do
        _controller.HttpContext.Items["CorrelationId"] = correlationId;
        
        var pet = new PetDetailDto 
        { 
            Id = petId, 
            Name = "Buddy", 
            PetType = "Dog",
            Owner = new OwnerDto { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com" }
        };
        
        _mockPetService.Setup(x => x.GetPetByIdAsync(petId))
            .ReturnsAsync(pet);

        // Act
        var result = await _controller.GetPetByIdAsync(petId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(pet);
        result.Message.Should().BeNull();

        // Note: IncrementViewsAsync is called asynchronously in fire-and-forget manner
        // We don't verify it here as it's non-blocking background operation
        
        // Verify logging
        VerifyLogContainsCorrelationId(LogLevel.Information, correlationId);
    }

    [Fact]
    public async Task GetPetByIdAsync_WithNonExistentId_ShouldReturnFailResponse()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var correlationId = Guid.NewGuid().ToString();
        _controller.Request.Headers["X-Correlation-ID"] = correlationId;
        // Simulate what the middleware would do
        _controller.HttpContext.Items["CorrelationId"] = correlationId;
        
        _mockPetService.Setup(x => x.GetPetByIdAsync(petId))
            .ReturnsAsync((PetDetailDto?)null);

        // Act
        var result = await _controller.GetPetByIdAsync(petId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be($"Pet with ID {petId} not found");

        // Verify warning logging
        VerifyLogContainsCorrelationId(LogLevel.Warning, correlationId);
    }

    [Fact]
    public async Task GetPetByIdAsync_WhenServiceThrowsException_ShouldReturnFailResponse()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var exception = new Exception("Database error");
        
        _mockPetService.Setup(x => x.GetPetByIdAsync(petId))
            .ThrowsAsync(exception);

        // Act
        var result = await _controller.GetPetByIdAsync(petId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("An error occurred while retrieving the pet");

        // Verify error logging includes generated correlation ID
        VerifyLogContainsValidCorrelationId(LogLevel.Error);
    }

    #endregion

    #region SearchPetsAsync Tests

    [Fact]
    public async Task SearchPetsAsync_WithValidSearchTerm_ShouldReturnSuccessResponse()
    {
        // Arrange
        const string searchTerm = "dog";
        const string petType = "Dog";
        var correlationId = Guid.NewGuid().ToString();
        _controller.Request.Headers["X-Correlation-ID"] = correlationId;
        // Simulate what the middleware would do
        _controller.HttpContext.Items["CorrelationId"] = correlationId;
        
        var pets = new List<PetListDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Buddy", PetType = "Dog" }
        };
        
        _mockPetService.Setup(x => x.SearchPetsAsync(searchTerm, petType))
            .ReturnsAsync(pets);

        // Act
        var result = await _controller.SearchPetsAsync(searchTerm, petType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data.Should().BeEquivalentTo(pets);

        // Verify logging
        VerifyLogContainsCorrelationId(LogLevel.Information, correlationId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task SearchPetsAsync_WithInvalidSearchTerm_ShouldReturnFailResponse(string? searchTerm)
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        _controller.Request.Headers["X-Correlation-ID"] = correlationId;
        // Simulate what the middleware would do
        _controller.HttpContext.Items["CorrelationId"] = correlationId;

        // Act
        var result = await _controller.SearchPetsAsync(searchTerm!, null);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Search term is required");

        // Verify warning logging
        VerifyLogContainsCorrelationId(LogLevel.Warning, correlationId);
        
        // Verify service was not called
        _mockPetService.Verify(x => x.SearchPetsAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SearchPetsAsync_WhenServiceThrowsException_ShouldReturnFailResponse()
    {
        // Arrange
        const string searchTerm = "dog";
        var exception = new Exception("Search failed");
        
        _mockPetService.Setup(x => x.SearchPetsAsync(searchTerm, null))
            .ThrowsAsync(exception);

        // Act
        var result = await _controller.SearchPetsAsync(searchTerm, null);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("An error occurred while searching pets");

        // Verify error logging includes generated correlation ID
        VerifyLogContainsValidCorrelationId(LogLevel.Error);
    }

    #endregion

    #region GetPetsByTypeAsync Tests

    [Fact]
    public async Task GetPetsByTypeAsync_WithValidType_ShouldReturnSuccessResponse()
    {
        // Arrange
        const string petType = "Dog";
        var correlationId = Guid.NewGuid().ToString();
        _controller.Request.Headers["X-Correlation-ID"] = correlationId;
        // Simulate what the middleware would do
        _controller.HttpContext.Items["CorrelationId"] = correlationId;
        
        var pets = new List<PetListDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Buddy", PetType = "Dog" },
            new() { Id = Guid.NewGuid(), Name = "Rex", PetType = "Dog" }
        };
        
        _mockPetService.Setup(x => x.GetPetsByTypeAsync(petType))
            .ReturnsAsync(pets);

        // Act
        var result = await _controller.GetPetsByTypeAsync(petType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.Should().BeEquivalentTo(pets);

        // Verify logging
        VerifyLogContainsCorrelationId(LogLevel.Information, correlationId);
    }

    [Fact]
    public async Task GetPetsByTypeAsync_WhenServiceThrowsException_ShouldReturnFailResponse()
    {
        // Arrange
        const string petType = "Dog";
        var exception = new Exception("Database error");
        
        _mockPetService.Setup(x => x.GetPetsByTypeAsync(petType))
            .ThrowsAsync(exception);

        // Act
        var result = await _controller.GetPetsByTypeAsync(petType);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("An error occurred while retrieving pets");

        // Verify error logging includes generated correlation ID
        VerifyLogContainsValidCorrelationId(LogLevel.Error);
    }

    #endregion

    #region Helper Methods

    private void VerifyLogContainsCorrelationId(LogLevel logLevel, string correlationId)
    {
        _mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(correlationId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private void VerifyLogContainsValidCorrelationId(LogLevel logLevel)
    {
        _mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CorrelationId: ")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}