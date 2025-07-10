using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetLoveCommunity.Domain.Entities;
using PetLoveCommunity.Infrastructure.Data;

namespace PetLoveCommunity.Infrastructure.Services;

public interface IDatabaseSeeder
{
    Task SeedAsync();
}

public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Check if we already have pets
            if (await _context.Pets.AnyAsync())
            {
                _logger.LogInformation("Database already contains pet data. Skipping seeding.");
                return;
            }

            _logger.LogInformation("Starting database seeding...");

            // Create default shelter user first
            var defaultShelter = await CreateDefaultShelterAsync();

            // Seed sample pets
            await SeedPetsAsync(defaultShelter.Id);

            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task<User> CreateDefaultShelterAsync()
    {
        var existingShelter = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == "shelter@petlove.com");

        if (existingShelter != null)
        {
            return existingShelter;
        }

        var defaultShelter = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Pet Love",
            LastName = "Shelter",
            Email = "shelter@petlove.com",
            PasswordHash = "hashed_password_placeholder", // In real app, use proper password hashing
            Role = UserRole.Shelter,
            Status = UserStatus.Active,
            IsEmailVerified = true,
            EmailVerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(defaultShelter);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created default shelter user with ID: {ShelterId}", defaultShelter.Id);
        return defaultShelter;
    }

    private async Task SeedPetsAsync(Guid ownerId)
    {
        var samplePets = SamplePetData.GetSamplePets();
        var pets = new List<Pet>();

        foreach (var samplePet in samplePets)
        {
            var pet = MapSamplePetToPet(samplePet, ownerId);
            pets.Add(pet);
        }

        _context.Pets.AddRange(pets);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} pets into the database.", pets.Count);
    }

    private static Pet MapSamplePetToPet(SamplePet samplePet, Guid ownerId)
    {
        return new Pet
        {
            Id = Guid.NewGuid(),
            Name = samplePet.Name,
            PetType = samplePet.PetType,
            Breed = samplePet.Breed,
            Image = samplePet.Image,
            DateOfBirth = DateTime.SpecifyKind(samplePet.DateOfBirth, DateTimeKind.Utc),
            Price = samplePet.Price,
            Description = samplePet.Description,
            Gender = MapGenderToPetGender(samplePet.Gender),
            AdoptionStatus = samplePet.AdoptionStatus,
            IsActive = samplePet.IsActive,
            Views = samplePet.Views,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    private static PetGender MapGenderToPetGender(Gender gender)
    {
        return gender switch
        {
            Gender.Male => PetGender.Male,
            Gender.Female => PetGender.Female,
            _ => PetGender.Unknown
        };
    }
}