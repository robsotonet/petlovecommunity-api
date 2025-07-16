using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PetLoveCommunity.API.Extensions;

namespace PetLoveCommunity.Tests.Unit.API.Extensions;

public class HttpContextExtensionsTests
{
    #region GetCorrelationId from HttpContext Tests

    [Fact]
    public void GetCorrelationId_WithNullContext_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => ((HttpContext)null!).GetCorrelationId();
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");
    }

    [Fact]
    public void GetCorrelationId_WithExistingCorrelationId_ShouldReturnStoredId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var expectedCorrelationId = "test-correlation-id";
        context.Items["CorrelationId"] = expectedCorrelationId;

        // Act
        var result = context.GetCorrelationId();

        // Assert
        result.Should().Be(expectedCorrelationId);
    }

    [Fact]
    public void GetCorrelationId_WithoutCorrelationId_ShouldGenerateAndReturnNewId()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var result = context.GetCorrelationId();

        // Assert
        result.Should().NotBeNullOrEmpty();
        Guid.TryParse(result, out _).Should().BeTrue();
        context.Items["CorrelationId"].Should().Be(result);
    }

    [Fact]
    public void GetCorrelationId_WithNullCorrelationIdItem_ShouldGenerateNewId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items["CorrelationId"] = null;

        // Act
        var result = context.GetCorrelationId();

        // Assert
        result.Should().NotBeNullOrEmpty();
        Guid.TryParse(result, out _).Should().BeTrue();
        context.Items["CorrelationId"].Should().Be(result);
    }

    [Fact]
    public void GetCorrelationId_WithEmptyStringCorrelationId_ShouldGenerateNewId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items["CorrelationId"] = "";

        // Act
        var result = context.GetCorrelationId();

        // Assert
        result.Should().NotBeNullOrEmpty();
        Guid.TryParse(result, out _).Should().BeTrue();
        context.Items["CorrelationId"].Should().Be(result);
    }

    [Fact]
    public void GetCorrelationId_WithWhitespaceCorrelationId_ShouldGenerateNewId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items["CorrelationId"] = "   ";

        // Act
        var result = context.GetCorrelationId();

        // Assert
        result.Should().NotBeNullOrEmpty();
        Guid.TryParse(result, out _).Should().BeTrue();
        context.Items["CorrelationId"].Should().Be(result);
    }

    [Fact]
    public void GetCorrelationId_WithNonStringCorrelationId_ShouldGenerateNewId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items["CorrelationId"] = 12345; // Non-string value

        // Act
        var result = context.GetCorrelationId();

        // Assert
        result.Should().NotBeNullOrEmpty();
        Guid.TryParse(result, out _).Should().BeTrue();
        context.Items["CorrelationId"].Should().Be(result);
    }

    [Theory]
    [InlineData("custom-correlation-id")]
    [InlineData("12345")]
    [InlineData("abc-def-ghi")]
    [InlineData("550e8400-e29b-41d4-a716-446655440000")]
    public void GetCorrelationId_WithVariousValidCorrelationIds_ShouldReturnOriginalId(string correlationId)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items["CorrelationId"] = correlationId;

        // Act
        var result = context.GetCorrelationId();

        // Assert
        result.Should().Be(correlationId);
    }

    [Fact]
    public void GetCorrelationId_CalledMultipleTimes_ShouldReturnSameIdAfterFirstGeneration()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var result1 = context.GetCorrelationId();
        var result2 = context.GetCorrelationId();
        var result3 = context.GetCorrelationId();

        // Assert
        result1.Should().Be(result2);
        result2.Should().Be(result3);
        result1.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region GetCorrelationId from ControllerBase Tests

    [Fact]
    public void GetCorrelationId_FromControllerBase_WithNullControllerBase_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => ((ControllerBase)null!).GetCorrelationId();
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("controllerBase");
    }

    [Fact]
    public void GetCorrelationId_FromControllerBase_WithValidController_ShouldReturnCorrelationId()
    {
        // Arrange
        var mockController = new Mock<ControllerBase>();
        var httpContext = new DefaultHttpContext();
        var expectedCorrelationId = "controller-correlation-id";
        httpContext.Items["CorrelationId"] = expectedCorrelationId;
        
        mockController.Object.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = mockController.Object.GetCorrelationId();

        // Assert
        result.Should().Be(expectedCorrelationId);
    }

    [Fact]
    public void GetCorrelationId_FromControllerBase_WithoutCorrelationId_ShouldGenerateNewId()
    {
        // Arrange
        var mockController = new Mock<ControllerBase>();
        var httpContext = new DefaultHttpContext();
        
        mockController.Object.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = mockController.Object.GetCorrelationId();

        // Assert
        result.Should().NotBeNullOrEmpty();
        Guid.TryParse(result, out _).Should().BeTrue();
        httpContext.Items["CorrelationId"].Should().Be(result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void GetCorrelationId_BothMethods_ShouldReturnSameIdForSameContext()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var expectedCorrelationId = "integration-test-id";
        httpContext.Items["CorrelationId"] = expectedCorrelationId;

        var mockController = new Mock<ControllerBase>();
        mockController.Object.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var resultFromContext = httpContext.GetCorrelationId();
        var resultFromController = mockController.Object.GetCorrelationId();

        // Assert
        resultFromContext.Should().Be(expectedCorrelationId);
        resultFromController.Should().Be(expectedCorrelationId);
        resultFromContext.Should().Be(resultFromController);
    }

    [Fact]
    public void GetCorrelationId_BothMethods_ShouldGenerateSameIdWhenNoneExists()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        var mockController = new Mock<ControllerBase>();
        mockController.Object.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var resultFromContext = httpContext.GetCorrelationId();
        var resultFromController = mockController.Object.GetCorrelationId();

        // Assert
        resultFromContext.Should().NotBeNullOrEmpty();
        resultFromController.Should().NotBeNullOrEmpty();
        resultFromContext.Should().Be(resultFromController);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void GetCorrelationId_WithEmptyItemsDictionary_ShouldGenerateNewId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items.Clear(); // Ensure items dictionary is empty

        // Act
        var result = context.GetCorrelationId();

        // Assert
        result.Should().NotBeNullOrEmpty();
        Guid.TryParse(result, out _).Should().BeTrue();
        context.Items["CorrelationId"].Should().Be(result);
    }

    [Fact]
    public async Task GetCorrelationId_ConcurrentAccess_ShouldHandleGracefully()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => context.GetCorrelationId()))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        var results = tasks.Select(t => t.Result).ToList();
        results.Should().AllSatisfy(id => id.Should().NotBeNullOrEmpty());
        
        // All results should be the same (first one generated wins)
        var firstResult = results.First();
        results.Should().AllSatisfy(id => id.Should().Be(firstResult));
    }

    #endregion
}