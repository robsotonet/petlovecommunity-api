using PetLoveCommunity.Application.DTOs;

namespace PetLoveCommunity.Application.Interfaces;

public interface IPetService
{
    Task<IEnumerable<PetListDto>> GetAllAvailablePetsAsync();
    Task<PetDetailDto?> GetPetByIdAsync(Guid id);
    Task<IEnumerable<PetListDto>> GetPetsByTypeAsync(string petType);
    Task<IEnumerable<PetListDto>> SearchPetsAsync(string searchTerm, string? petType = null);
    Task<IEnumerable<PetListDto>> GetPetsByOwnerAsync(Guid ownerId);
    Task<PetDetailDto> CreatePetAsync(PetDetailDto petDto);
    Task<PetDetailDto> UpdatePetAsync(Guid id, PetDetailDto petDto);
    Task<bool> DeletePetAsync(Guid id);
    Task IncrementViewsAsync(Guid id);
}