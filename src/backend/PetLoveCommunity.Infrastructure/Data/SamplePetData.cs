using PetLoveCommunity.Domain.Entities;

namespace PetLoveCommunity.Infrastructure.Data;

public static class SamplePetData
{
    public static List<SamplePet> GetSamplePets()
    {
        return new List<SamplePet>
        {
            new SamplePet { PetId = 2, Name = "Mittens", PetType = PetType.Cat, Breed = "Devon Rex", Image = "image_09.png", DateOfBirth = new DateTime(2019, 3, 15), Price = 300.00m, Description = "Affectionate and vocal", Gender = Gender.Female, IsActive = true, AdoptionStatus = AdoptionStatus.Available, Views = 0 },
            new SamplePet { PetId = 3, Name = "Bella", PetType = PetType.Dog, Breed = "Labrador Retriever", Image = "image_02.png", DateOfBirth = new DateTime(2018, 8, 12), Price = 600.00m, Description = "Loyal and friendly", Gender = Gender.Female, IsActive = true, AdoptionStatus = AdoptionStatus.Available, Views = 0 },
            new SamplePet { PetId = 4, Name = "Whiskers", PetType = PetType.Cat, Breed = "Persian", Image = "image_10.png", DateOfBirth = new DateTime(2019, 4, 22), Price = 400.00m, Description = "Quiet and affectionate", Gender = Gender.Male, IsActive = true, AdoptionStatus = AdoptionStatus.Available, Views = 0 },
            new SamplePet { PetId = 5, Name = "Max", PetType = PetType.Dog, Breed = "German Shepherd", Image = "image_03.png", DateOfBirth = new DateTime(2018, 10, 25), Price = 700.00m, Description = "Loyal and protective", Gender = Gender.Male, IsActive = true, AdoptionStatus = AdoptionStatus.Available, Views = 0},
            new SamplePet { PetId = 6,  Name = "Luna", PetType = PetType.Cat, Breed = "Maine Coon", Image = "image_11.png", DateOfBirth = new DateTime(2019, 5, 5), Price = 450.00m, Description = "Gentle and playful", Gender = Gender.Female , IsActive = true, AdoptionStatus = AdoptionStatus.Available, Views = 0},
            new SamplePet { PetId = 7, Name = "Shadow", PetType = PetType.Cat, Breed = "Ragdoll", Image = "image_12.png", DateOfBirth = new DateTime(2019, 6, 25), Price = 350.00m, Description = "Calm and affectionate", Gender = Gender.Male, IsActive = true, AdoptionStatus = AdoptionStatus.Available, Views = 0 },
            new SamplePet { PetId = 8, Name = "Charlie", PetType = PetType.Dog, Breed = "Poodle", Image = "image_04.png", DateOfBirth = new DateTime(2018, 11, 15), Price = 550.00m, Description = "Intelligent and friendly", Gender = Gender.Male, IsActive = true, AdoptionStatus = AdoptionStatus.Available, Views = 0 },
            new SamplePet { PetId = 9, Name = "Tweety", PetType = PetType.Dog, Breed = "Golden Retriever", Image = "image_05.png", DateOfBirth = new DateTime(2018, 5, 1), Price = 500.00m, Description = "Friendly and energetic", Gender = Gender.Male, IsActive = true, AdoptionStatus = AdoptionStatus.Available, Views = 0},
            new SamplePet { PetId = 10, Name = "Nemo", PetType = PetType.Cat, Breed = "Abyssinian", Image = "image_13.png", DateOfBirth = new DateTime(2019, 3, 15), Price = 300.00m, Description = "Affectionate and vocal", Gender = Gender.Female, IsActive = true, AdoptionStatus = AdoptionStatus.Available, Views = 0 },
            new SamplePet { PetId = 11, Name = "Bubbles", PetType = PetType.Cat, Breed = "Abyssinian", Image = "image_14.png", DateOfBirth = new DateTime(2019, 6, 25), Price = 350.00m, Description = "Calm and affectionate", Gender = Gender.Male, IsActive = true, AdoptionStatus = AdoptionStatus.Available, Views = 0 },
            new SamplePet { PetId = 12, Name = "Kiwi", PetType = PetType.Dog, Breed = "Poodle", Image = "image_06.png", DateOfBirth = new DateTime(2018, 11, 15), Price = 550.00m, Description = "Intelligent and friendly", Gender = Gender.Male, IsActive = true, AdoptionStatus = AdoptionStatus.Available, Views = 0 },
            new SamplePet { PetId = 13, Name = "Spike", PetType = PetType.Dog, Breed = "Boxer", Image = "image_07.png", DateOfBirth = new DateTime(2018, 8, 12), Price = 600.00m, Description = "Loyal and friendly", Gender = Gender.Female, IsActive = true, AdoptionStatus = AdoptionStatus.Available, Views = 0 },
            new SamplePet { PetId = 14, Name = "Dory", PetType = PetType.Cat, Breed = "Devon Rex", Image = "image_15.png", DateOfBirth = new DateTime(2019, 4, 22), Price = 400.00m, Description = "Quiet and affectionate", Gender = Gender.Male, IsActive = true, AdoptionStatus = AdoptionStatus.Available, Views = 0 },
            new SamplePet { PetId = 15, Name = "Rex", PetType = PetType.Dog, Breed = "Beagle", Image = "image_08.png", DateOfBirth = new DateTime(2018, 10, 25), Price = 700.00m, Description = "Loyal and protective", Gender = Gender.Male, IsActive = true, AdoptionStatus = AdoptionStatus.Available, Views = 0},
            new SamplePet { PetId = 16, Name = "Sunny", PetType = PetType.Cat, Breed = "Domestic Shorthair", Image = "image_16.png", DateOfBirth = new DateTime(2019, 5, 5), Price = 450.00m, Description = "Gentle and playful", Gender = Gender.Female , IsActive = true, AdoptionStatus = AdoptionStatus.Available, Views = 0}
        };
    }
}

public class SamplePet
{
    public int PetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public PetType PetType { get; set; }
    public string Breed { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public bool IsActive { get; set; }
    public AdoptionStatus AdoptionStatus { get; set; }
    public int Views { get; set; }
}

public enum Gender
{
    Male = 0,
    Female = 1
}