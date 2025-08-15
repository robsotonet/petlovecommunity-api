using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using PetLoveCommunity.API.Middleware;

namespace PetLoveCommunity.Tests.Unit.API.Middleware;

public class CorrelationIdMiddlewareTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<ILogger<CorrelationIdMiddleware>> _mockLogger;
    private readonly CorrelationIdMiddleware _middleware;

    public CorrelationIdMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLogger = new Mock<ILogger<CorrelationIdMiddleware>>();
        _middleware = new CorrelationIdMiddleware(_mockNext.Object, _mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullNext_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new CorrelationIdMiddleware(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("next");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new CorrelationIdMiddleware(_mockNext.Object, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var middleware = new CorrelationIdMiddleware(_mockNext.Object, _mockLogger.Object);

        // Assert
        middleware.Should().NotBeNull();
    }

    #endregion

    #region InvokeAsync Tests

    [Fact]
    public async Task InvokeAsync_WithNullContext_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = async () => await _middleware.InvokeAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("context");
    }

    [Fact]
    public async Task InvokeAsync_WithExistingCorrelationId_ShouldUseProvidedId()
    {
        // Arrange
        var context = CreateHttpContext();
        var existingCorrelationId = "existing-correlation-id";
        context.Request.Headers.Append("X-Correlation-ID", existingCorrelationId);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Items["CorrelationId"].Should().Be(existingCorrelationId);
        context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(existingCorrelationId);
        _mockNext.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithoutCorrelationId_ShouldGenerateNewId()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Items["CorrelationId"] as string;
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue();
        context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(correlationId);
        _mockNext.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyCorrelationId_ShouldGenerateNewId()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers.Append("X-Correlation-ID", "");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Items["CorrelationId"] as string;
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue();
        context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(correlationId);
        _mockNext.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithWhitespaceCorrelationId_ShouldGenerateNewId()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers.Append("X-Correlation-ID", "   ");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Items["CorrelationId"] as string;
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue();
        context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(correlationId);
        _mockNext.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers.Append("X-Correlation-ID", "test-correlation-id");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddCorrelationIdToResponseHeaders()
    {
        // Arrange
        var context = CreateHttpContext();
        var testCorrelationId = "test-correlation-id";
        context.Request.Headers.Append("X-Correlation-ID", testCorrelationId);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("X-Correlation-ID");
        context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(testCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldStoreCorrelationIdInHttpContextItems()
    {
        // Arrange
        var context = CreateHttpContext();
        var testCorrelationId = "test-correlation-id";
        context.Request.Headers.Append("X-Correlation-ID", testCorrelationId);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().ContainKey("CorrelationId");
        context.Items["CorrelationId"].Should().Be(testCorrelationId);
    }

    [Theory]
    [InlineData("custom-correlation-id")]
    [InlineData("12345")]
    [InlineData("abc-def-ghi")]
    [InlineData("550e8400-e29b-41d4-a716-446655440000")]
    public async Task InvokeAsync_WithVariousCorrelationIdFormats_ShouldPreserveOriginalId(string correlationId)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers.Append("X-Correlation-ID", correlationId);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Items["CorrelationId"].Should().Be(correlationId);
        context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(correlationId);
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task InvokeAsync_WithExistingCorrelationId_ShouldLogDebugMessage()
    {
        // Arrange
        var context = CreateHttpContext();
        var correlationId = "test-correlation-id";
        context.Request.Headers.Append("X-Correlation-ID", correlationId);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        VerifyLogCalled(LogLevel.Debug, "Using correlation ID from request header");
    }

    [Fact]
    public async Task InvokeAsync_WithGeneratedCorrelationId_ShouldLogDebugMessage()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        VerifyLogCalled(LogLevel.Debug, "Generated new correlation ID");
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task InvokeAsync_WhenNextMiddlewareThrows_ShouldPropagateException()
    {
        // Arrange
        var context = CreateHttpContext();
        var expectedException = new InvalidOperationException("Test exception");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(expectedException);

        // Act & Assert
        var act = async () => await _middleware.InvokeAsync(context);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");

        // Correlation ID should still be set
        context.Items["CorrelationId"].Should().NotBeNull();
        context.Response.Headers.Should().ContainKey("X-Correlation-ID");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task InvokeAsync_MultipleRequests_ShouldGenerateUniqueCorrelationIds()
    {
        // Arrange
        var context1 = CreateHttpContext();
        var context2 = CreateHttpContext();
        var context3 = CreateHttpContext();

        // Act
        await _middleware.InvokeAsync(context1);
        await _middleware.InvokeAsync(context2);
        await _middleware.InvokeAsync(context3);

        // Assert
        var correlationId1 = context1.Items["CorrelationId"] as string;
        var correlationId2 = context2.Items["CorrelationId"] as string;
        var correlationId3 = context3.Items["CorrelationId"] as string;

        correlationId1.Should().NotBeNullOrEmpty();
        correlationId2.Should().NotBeNullOrEmpty();
        correlationId3.Should().NotBeNullOrEmpty();

        correlationId1.Should().NotBe(correlationId2);
        correlationId1.Should().NotBe(correlationId3);
        correlationId2.Should().NotBe(correlationId3);
    }

    [Fact]
    public async Task InvokeAsync_RequestWithMixedCorrelationIdSources_ShouldHandleCorrectly()
    {
        // Arrange
        var contextWithHeader = CreateHttpContext();
        contextWithHeader.Request.Headers.Append("X-Correlation-ID", "from-header");

        var contextWithoutHeader = CreateHttpContext();

        // Act
        await _middleware.InvokeAsync(contextWithHeader);
        await _middleware.InvokeAsync(contextWithoutHeader);

        // Assert
        contextWithHeader.Items["CorrelationId"].Should().Be("from-header");
        contextWithoutHeader.Items["CorrelationId"].Should().NotBe("from-header");
        var correlationIdWithoutHeader = contextWithoutHeader.Items["CorrelationId"] as string;
        Guid.TryParse(correlationIdWithoutHeader, out _).Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("localhost");
        context.Request.Path = "/api/test";
        return context;
    }

    private void VerifyLogCalled(LogLevel logLevel, string messageContains)
    {
        _mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(messageContains)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}