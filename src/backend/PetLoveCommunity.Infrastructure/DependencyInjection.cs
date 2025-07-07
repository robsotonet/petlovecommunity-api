using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetLoveCommunity.Application.Configuration;
using PetLoveCommunity.Domain.Interfaces;
using PetLoveCommunity.Infrastructure.Data;
using PetLoveCommunity.Infrastructure.Repositories;

namespace PetLoveCommunity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Get configuration sections
        var databaseSettings = configuration.GetSection("Database").Get<DatabaseSettings>()
            ?? throw new InvalidOperationException("Database configuration is missing");
        
        var databaseCredentials = configuration.GetSection("DatabaseCredentials").Get<DatabaseCredentials>()
            ?? throw new InvalidOperationException("Database credentials configuration is missing");

        // Build connection string
        var connectionString = ConnectionStringBuilder.BuildConnectionString(databaseSettings, databaseCredentials);

        // Register DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });

            // Enable sensitive data logging in development
            if (configuration.GetValue<bool>("Database:IncludeErrorDetail", false))
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPetRepository, PetRepository>();

        return services;
    }
}