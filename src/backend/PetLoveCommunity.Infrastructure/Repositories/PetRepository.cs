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
            .Where(p => p.Status == PetStatus.Available && p.ExpiresAt > DateTime.UtcNow)
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
            .Where(p => p.Type == type && p.Status == PetStatus.Available)
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
        PetSize? size = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(p => p.Status == PetStatus.Available && p.ExpiresAt > DateTime.UtcNow)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => 
                p.Name.Contains(searchTerm) || 
                p.Breed.Contains(searchTerm) ||
                p.Description!.Contains(searchTerm));
        }

        if (type.HasValue)
        {
            query = query.Where(p => p.Type == type.Value);
        }

        if (size.HasValue)
        {
            query = query.Where(p => p.Size == size.Value);
        }

        return await query
            .Include(p => p.Owner)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Pet>> GetExpiredListingsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.ExpiresAt <= DateTime.UtcNow && p.Status != PetStatus.Expired)
            .ToListAsync(cancellationToken);
    }
}