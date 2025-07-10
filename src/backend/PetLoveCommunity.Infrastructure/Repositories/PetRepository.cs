using Microsoft.EntityFrameworkCore;
using PetLoveCommunity.Domain.Entities;
using PetLoveCommunity.Domain.Interfaces;
using PetLoveCommunity.Infrastructure.Data;

namespace PetLoveCommunity.Infrastructure.Repositories;

public class PetRepository : Repository<Pet>, IPetRepository
{
    public PetRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Pet>> GetAvailablePetsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.AdoptionStatus == AdoptionStatus.Available && p.IsActive)
            .Include(p => p.Owner)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Pet>> GetPetsByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.OwnerId == ownerId)
            .Include(p => p.Photos)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Pet>> GetPetsByTypeAsync(PetType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.PetType == type && p.AdoptionStatus == AdoptionStatus.Available && p.IsActive)
            .Include(p => p.Owner)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Pet?> GetWithPhotosAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Photos.OrderBy(ph => ph.DisplayOrder))
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == petId, cancellationToken);
    }

    public async Task<IEnumerable<Pet>> SearchPetsAsync(
        string searchTerm, 
        PetType? type = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(p => p.AdoptionStatus == AdoptionStatus.Available && p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => 
                p.Name.Contains(searchTerm) || 
                p.Breed.Contains(searchTerm) ||
                p.Description.Contains(searchTerm));
        }

        if (type.HasValue)
        {
            query = query.Where(p => p.PetType == type.Value);
        }

        return await query
            .Include(p => p.Owner)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Pet>> GetPetsByStatusAsync(AdoptionStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.AdoptionStatus == status)
            .Include(p => p.Owner)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}