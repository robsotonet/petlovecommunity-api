using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PetLoveCommunity.Application.Configuration;
using PetLoveCommunity.API.Services;
using PetLoveCommunity.Infrastructure;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Enhanced logging configuration for debugging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}

// Configure JWT settings
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddSingleton<IJwtSettings>(provider => 
    provider.GetRequiredService<IOptions<JwtSettings>>().Value);

// Configure Database settings
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(DatabaseSettings.SectionName));
builder.Services.Configure<DatabaseCredentials>(builder.Configuration.GetSection(DatabaseCredentials.SectionName));
builder.Services.Configure<DatabaseAdminCredentials>(builder.Configuration.GetSection(DatabaseAdminCredentials.SectionName));

// Register configuration interfaces
builder.Services.AddSingleton<IDatabaseSettings>(provider => 
    provider.GetRequiredService<IOptions<DatabaseSettings>>().Value);
builder.Services.AddSingleton<IDatabaseCredentials>(provider => 
    provider.GetRequiredService<IOptions<DatabaseCredentials>>().Value);
builder.Services.AddSingleton<IDatabaseAdminCredentials>(provider => 
    provider.GetRequiredService<IOptions<DatabaseAdminCredentials>>().Value);

// Register services
builder.Services.AddSingleton<IConfigurationValidator, ConfigurationValidator>();
builder.Services.AddSingleton<IDatabaseConnectionService, DatabaseConnectionService>();

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Add request logging middleware for debugging
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogDebug("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
        await next();
        logger.LogDebug("Response: {StatusCode}", context.Response.StatusCode);
    });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PetLoveCommunity API v1"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
