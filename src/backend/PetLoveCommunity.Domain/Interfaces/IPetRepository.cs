using PetLoveCommunity.Domain.Entities;

namespace PetLoveCommunity.Domain.Interfaces;

public interface IPetRepository : IRepository<Pet>
{
    Task<IEnumerable<Pet>> GetAvailablePetsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Pet>> GetPetsByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Pet>> GetPetsByTypeAsync(PetType type, CancellationToken cancellationToken = default);
    Task<Pet?> GetWithPhotosAsync(Guid petId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Pet>> SearchPetsAsync(string searchTerm, PetType? type = null, PetSize? size = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Pet>> GetExpiredListingsAsync(CancellationToken cancellationToken = default);
}