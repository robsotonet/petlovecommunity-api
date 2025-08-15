namespace PetLoveCommunity.API.Extensions;

/// <summary>
/// Extension methods for HttpContext to provide easy access to correlation IDs and other context data.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Gets the correlation ID from the current HTTP context.
    /// The correlation ID is set by the CorrelationIdMiddleware and stored in HttpContext.Items.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>
    /// The correlation ID string if available, otherwise generates a new GUID.
    /// This fallback ensures a correlation ID is always available even if the middleware is not configured.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    public static string GetCorrelationId(this HttpContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        // Try to get correlation ID from HttpContext.Items (set by middleware)
        if (context.Items.TryGetValue("CorrelationId", out var correlationId) && 
            correlationId is string correlationIdString && 
            !string.IsNullOrWhiteSpace(correlationIdString))
        {
            return correlationIdString;
        }

        // Fallback: generate a new correlation ID if not found
        // This should only happen if the middleware is not configured
        var fallbackCorrelationId = Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = fallbackCorrelationId;
        
        return fallbackCorrelationId;
    }

    /// <summary>
    /// Gets the correlation ID from the current HTTP context for controller usage.
    /// This is a convenience method for controllers to access the correlation ID.
    /// </summary>
    /// <param name="controllerBase">The controller base instance.</param>
    /// <returns>The correlation ID string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when controllerBase is null.</exception>
    public static string GetCorrelationId(this Microsoft.AspNetCore.Mvc.ControllerBase controllerBase)
    {
        if (controllerBase == null) throw new ArgumentNullException(nameof(controllerBase));

        return controllerBase.HttpContext.GetCorrelationId();
    }
}