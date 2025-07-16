namespace PetLoveCommunity.API.Middleware;

/// <summary>
/// Middleware to handle correlation IDs for request tracing across the application.
/// Automatically generates a correlation ID if one is not provided in the request headers.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the CorrelationIdMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">Logger for middleware operations.</param>
    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes the HTTP request to extract or generate a correlation ID.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        string correlationId;

        // Try to get correlation ID from request headers
        if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationIdHeader) &&
            !string.IsNullOrWhiteSpace(correlationIdHeader))
        {
            correlationId = correlationIdHeader.ToString();
            _logger.LogDebug("Using correlation ID from request header: {CorrelationId}", correlationId);
        }
        else
        {
            // Generate a new correlation ID if not provided
            correlationId = Guid.NewGuid().ToString();
            _logger.LogDebug("Generated new correlation ID: {CorrelationId}", correlationId);
        }

        // Store correlation ID in HttpContext.Items for easy access by controllers
        context.Items["CorrelationId"] = correlationId;

        // Add correlation ID to response headers for client tracking
        context.Response.Headers.Append("X-Correlation-ID", correlationId);

        // Continue to next middleware
        await _next(context);
    }
}