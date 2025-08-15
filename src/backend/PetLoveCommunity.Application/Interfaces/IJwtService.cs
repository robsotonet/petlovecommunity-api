using PetLoveCommunity.Domain.Entities;

namespace PetLoveCommunity.Application.Interfaces;

public interface IJwtService
{
    string GenerateJwtToken(User user);
}