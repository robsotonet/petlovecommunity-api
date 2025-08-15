using Microsoft.Extensions.Logging;
using PetLoveCommunity.Application.DTOs;
using PetLoveCommunity.Application.Extensions;
using PetLoveCommunity.Application.Interfaces;
using PetLoveCommunity.Domain.Entities;
using PetLoveCommunity.Domain.Interfaces;

namespace PetLoveCommunity.Application.Services;

public class PetService : IPetService
{
    private readonly IPetRepository _petRepository;
    private readonly IAppConfig _appConfig;
    private readonly ILogger<PetService> _logger;

    public PetService(IPetRepository petRepository, IAppConfig appConfig, ILogger<PetService> logger)
    {
        _petRepository = petRepository;
        _appConfig = appConfig;
        _logger = logger;
    }

    public async Task<IEnumerable<PetListDto>> GetAllAvailablePetsAsync()
    {
        try
        {
            var pets = await _petRepository.GetAvailablePetsAsync();
            var result = pets.MapToPetListDtos(_appConfig.BaseApiUrl);
            _logger.LogInformation("Retrieved {Count} available pets", pets.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving available pets");
            throw;
        }
    }

    public async Task<PetDetailDto?> GetPetByIdAsync(Guid id)
    {
        try
        {
            var pet = await _petRepository.GetWithPhotosAsync(id);
            if (pet == null)
            {
                _logger.LogWarning("Pet with ID {PetId} not found", id);
                return null;
            }

            var result = pet.MapToPetDetailDto(_appConfig.BaseApiUrl);
            _logger.LogInformation("Retrieved pet details for ID {PetId}", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving pet with ID {PetId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<PetListDto>> GetPetsByTypeAsync(string petType)
    {
        try
        {
            if (!Enum.TryParse<PetType>(petType, true, out var type))
            {
                _logger.LogWarning("Invalid pet type: {PetType}", petType);
                return Enumerable.Empty<PetListDto>();
            }

            var pets = await _petRepository.GetPetsByTypeAsync(type);
            var result = pets.MapToPetListDtos(_appConfig.BaseApiUrl);
            _logger.LogInformation("Retrieved {Count} pets of type {PetType}", pets.Count(), petType);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving pets of type {PetType}", petType);
            throw;
        }
    }

    public async Task<IEnumerable<PetListDto>> SearchPetsAsync(string searchTerm, string? petType = null)
    {
        try
        {
            PetType? type = null;
            if (!string.IsNullOrEmpty(petType) && Enum.TryParse<PetType>(petType, true, out var parsedType))
            {
                type = parsedType;
            }

            var pets = await _petRepository.SearchPetsAsync(searchTerm, type);
            var result = pets.MapToPetListDtos(_appConfig.BaseApiUrl);
            _logger.LogInformation("Search for '{SearchTerm}' returned {Count} pets", searchTerm, pets.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching pets with term '{SearchTerm}'", searchTerm);
            throw;
        }
    }

    public async Task<IEnumerable<PetListDto>> GetPetsByOwnerAsync(Guid ownerId)
    {
        try
        {
            var pets = await _petRepository.GetPetsByOwnerAsync(ownerId);
            var result = pets.MapToPetListDtos(_appConfig.BaseApiUrl);
            _logger.LogInformation("Retrieved {Count} pets for owner {OwnerId}", pets.Count(), ownerId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving pets for owner {OwnerId}", ownerId);
            throw;
        }
    }

    public Task<PetDetailDto> CreatePetAsync(PetDetailDto petDto)
    {
        try
        {
            // TODO: Implement mapping from DTO to entity and create logic
            _logger.LogInformation("Creating new pet: {PetName}", petDto.Name);
            throw new NotImplementedException("CreatePetAsync not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating pet: {PetName}", petDto.Name);
            throw;
        }
    }

    public Task<PetDetailDto> UpdatePetAsync(Guid id, PetDetailDto petDto)
    {
        try
        {
            // TODO: Implement update logic
            _logger.LogInformation("Updating pet with ID {PetId}", id);
            throw new NotImplementedException("UpdatePetAsync not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating pet with ID {PetId}", id);
            throw;
        }
    }

    public Task<bool> DeletePetAsync(Guid id)
    {
        try
        {
            // TODO: Implement soft delete logic
            _logger.LogInformation("Deleting pet with ID {PetId}", id);
            throw new NotImplementedException("DeletePetAsync not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting pet with ID {PetId}", id);
            throw;
        }
    }

    public async Task IncrementViewsAsync(Guid id)
    {
        try
        {
            var pet = await _petRepository.GetByIdAsync(id);
            if (pet != null)
            {
                pet.Views++;
                await _petRepository.UpdateAsync(pet);
                _logger.LogDebug("Incremented views for pet {PetId} to {Views}", id, pet.Views);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while incrementing views for pet {PetId}", id);
            // Don't throw here as this is not critical functionality
        }
    }
}