using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PetLoveCommunity.API.Middleware;

namespace PetLoveCommunity.Tests.Unit.API.Middleware;

public class MiddlewareExtensionsTests
{
    #region UseCorrelationId Tests

    [Fact]
    public void UseCorrelationId_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => ((IApplicationBuilder)null!).UseCorrelationId();
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("builder");
    }

    [Fact]
    public void UseCorrelationId_WithValidBuilder_ShouldReturnSameBuilderInstance()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var builder = new ApplicationBuilder(serviceProvider);

        // Act
        var result = builder.UseCorrelationId();

        // Assert
        result.Should().Be(builder);
    }

    [Fact]
    public void UseCorrelationId_ExtensionMethod_ShouldExist()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var builder = new ApplicationBuilder(serviceProvider);

        // Act & Assert
        // This test verifies the extension method compiles and can be called
        var act = () => builder.UseCorrelationId();
        act.Should().NotThrow();
    }

    #endregion
}