using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLoveCommunity.Infrastructure.Data;

namespace PetLoveCommunity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Database connectivity health check
    /// </summary>
    [HttpGet("database")]
    public async Task<IActionResult> GetDatabaseHealth()
    {
        try
        {
            _logger.LogInformation("Checking database connectivity...");
            
            // Test basic database connectivity
            var canConnect = await _context.Database.CanConnectAsync();
            
            if (!canConnect)
            {
                _logger.LogError("Database connection failed");
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    error = "Cannot connect to database",
                    timestamp = DateTime.UtcNow
                });
            }

            // Test a simple query
            var userCount = await _context.Users.CountAsync();
            
            _logger.LogInformation("Database connectivity successful. User count: {UserCount}", userCount);
            
            return Ok(new
            {
                status = "healthy",
                database = new
                {
                    connectionStatus = "connected",
                    userCount = userCount,
                    databaseName = _context.Database.GetDbConnection().Database,
                    serverVersion = await GetDatabaseVersion()
                },
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            
            return StatusCode(503, new
            {
                status = "unhealthy",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Detailed system health check including all dependencies
    /// </summary>
    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailedHealth()
    {
        var healthChecks = new List<object>();

        // Database check
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            var userCount = canConnect ? await _context.Users.CountAsync() : 0;
            
            healthChecks.Add(new
            {
                component = "database",
                status = canConnect ? "healthy" : "unhealthy",
                details = new
                {
                    connectionStatus = canConnect ? "connected" : "disconnected",
                    userCount = userCount,
                    databaseName = _context.Database.GetDbConnection().Database
                }
            });
        }
        catch (Exception ex)
        {
            healthChecks.Add(new
            {
                component = "database",
                status = "unhealthy",
                error = ex.Message
            });
        }

        // Memory check
        var workingSet = Environment.WorkingSet;
        healthChecks.Add(new
        {
            component = "memory",
            status = workingSet < 500_000_000 ? "healthy" : "warning", // 500MB threshold
            details = new
            {
                workingSetBytes = workingSet,
                workingSetMB = workingSet / 1024 / 1024
            }
        });

        var overallStatus = healthChecks.All(h => h.GetType().GetProperty("status")?.GetValue(h)?.ToString() == "healthy") 
            ? "healthy" : "degraded";

        return Ok(new
        {
            status = overallStatus,
            checks = healthChecks,
            timestamp = DateTime.UtcNow
        });
    }

    private async Task<string> GetDatabaseVersion()
    {
        try
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT version()";
            var result = await command.ExecuteScalarAsync();
            return result?.ToString() ?? "Unknown";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not retrieve database version");
            return "Unknown";
        }
    }
}