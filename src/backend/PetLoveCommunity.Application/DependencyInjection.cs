using Microsoft.Extensions.DependencyInjection;
using PetLoveCommunity.Application.Interfaces;
using PetLoveCommunity.Application.Services;

namespace PetLoveCommunity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<IPetService, PetService>();
        services.AddScoped<IAppConfig, AppConfig>();
        
        // Register authentication services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}