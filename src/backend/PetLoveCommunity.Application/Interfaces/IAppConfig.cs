namespace PetLoveCommunity.Application.Interfaces;

public interface IAppConfig
{
    string BaseApiUrl { get; }
    string StaticFilesPath { get; }
    string PetImagesPath { get; }
}