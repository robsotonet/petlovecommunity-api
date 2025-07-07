using PetLoveCommunity.Domain.Entities;

namespace PetLoveCommunity.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
    Task<User?> GetWithVendorAsync(Guid userId, CancellationToken cancellationToken = default);
}