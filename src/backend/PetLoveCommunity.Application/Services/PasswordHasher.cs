using PetLoveCommunity.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace PetLoveCommunity.Application.Services;

public class PasswordHasher : IPasswordHasher
{
    public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty or whitespace only string.", nameof(password));

        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty or whitespace only string.", nameof(password));
        
        if (passwordHash.Length != 64)
            throw new ArgumentException("Invalid length of password hash (64 bytes expected).", nameof(passwordHash));
        
        if (passwordSalt.Length != 128)
            throw new ArgumentException("Invalid length of password salt (128 bytes expected).", nameof(passwordSalt));

        using var hmac = new HMACSHA512(passwordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        
        return computedHash.SequenceEqual(passwordHash);
    }
}