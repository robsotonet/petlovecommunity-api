namespace PetLoveCommunity.API.Middleware;

/// <summary>
/// Extension methods for configuring middleware in the application pipeline.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds the correlation ID middleware to the application pipeline.
    /// This middleware should be registered early in the pipeline to ensure
    /// correlation IDs are available for all subsequent middleware and controllers.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for method chaining.</returns>
    /// <remarks>
    /// This middleware:
    /// - Extracts correlation ID from X-Correlation-ID request header
    /// - Generates a new GUID if no correlation ID is provided
    /// - Stores the correlation ID in HttpContext.Items["CorrelationId"]
    /// - Adds the correlation ID to the X-Correlation-ID response header
    /// </remarks>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}