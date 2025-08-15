using Microsoft.Extensions.Configuration;
using PetLoveCommunity.Application.Interfaces;

namespace PetLoveCommunity.Application.Services;

public class AppConfig : IAppConfig
{
    private readonly IConfiguration _configuration;

    public AppConfig(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string BaseApiUrl => _configuration["AppConfig:BaseApiUrl"] ?? "http://localhost:5248";

    public string StaticFilesPath => "wwwroot";

    public string PetImagesPath => "images/pets";
}