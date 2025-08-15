using Microsoft.EntityFrameworkCore;
using PetLoveCommunity.Application.Interfaces;
using PetLoveCommunity.Domain.Entities;
using PetLoveCommunity.Infrastructure.Data;

namespace PetLoveCommunity.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task<User?> Authenticate(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user == null || !_passwordHasher.VerifyPasswordHash(password, Convert.FromBase64String(user.PasswordHash), user.PasswordSalt))
            return null;

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task Register(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(user.Password))
            throw new ArgumentException("Password is required", nameof(user));

        // Check if user already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists");

        // Hash the password
        _passwordHasher.CreatePasswordHash(user.Password, out byte[] passwordHash, out byte[] passwordSalt);
        user.PasswordHash = Convert.ToBase64String(passwordHash);
        user.PasswordSalt = passwordSalt;
        user.Password = "****"; // Clear the plain text password

        // Set default values
        user.Status = UserStatus.Active;
        user.Role = UserRole.Free;
        user.IsEmailVerified = false;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<User>> GetAllUsers()
    {
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserById(Guid id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }
}