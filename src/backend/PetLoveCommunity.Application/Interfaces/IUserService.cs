using PetLoveCommunity.Domain.Entities;

namespace PetLoveCommunity.Application.Interfaces;

public interface IUserService
{
    Task<User?> Authenticate(string email, string password);
    Task Register(User user);
    Task<IEnumerable<User>> GetAllUsers();
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserById(Guid id);
}