using PetLoveCommunity.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.Domain.Entities;

public class Pet : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public PetType Type { get; set; }

    [Required]
    [StringLength(100)]
    public string Breed { get; set; } = string.Empty;

    [Required]
    public PetGender Gender { get; set; }

    [Required]
    public int Age { get; set; }

    [Required]
    public PetSize Size { get; set; }

    [Required]
    [StringLength(50)]
    public string Color { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public PetStatus Status { get; set; } = PetStatus.Available;

    [Required]
    public bool IsVaccinated { get; set; } = false;

    [Required]
    public bool IsSpayedNeutered { get; set; } = false;

    [Required]
    public bool IsHouseTrained { get; set; } = false;

    [Required]
    public bool IsGoodWithKids { get; set; } = false;

    [Required]
    public bool IsGoodWithPets { get; set; } = false;

    [Required]
    public decimal AdoptionFee { get; set; } = 0;

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(255)]
    public string? PrimaryPhotoUrl { get; set; }

    [StringLength(500)]
    public string? SpecialNeeds { get; set; }

    public DateTime? AdoptedAt { get; set; }

    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(90);

    // Foreign keys
    [Required]
    public Guid OwnerId { get; set; }

    public Guid? AdoptedById { get; set; }

    // Navigation properties
    public virtual User Owner { get; set; } = null!;
    public virtual User? AdoptedBy { get; set; }
    public virtual ICollection<PetPhoto> Photos { get; set; } = new List<PetPhoto>();
    public virtual ICollection<UserPetFavorite> FavoriteByUsers { get; set; } = new List<UserPetFavorite>();
    public virtual ICollection<AdoptionApplication> AdoptionApplications { get; set; } = new List<AdoptionApplication>();
}

public class PetPhoto : BaseEntity
{
    [Required]
    [StringLength(255)]
    public string PhotoUrl { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Caption { get; set; }

    [Required]
    public int DisplayOrder { get; set; } = 0;

    [Required]
    public bool IsPrimary { get; set; } = false;

    // Foreign key
    [Required]
    public Guid PetId { get; set; }

    // Navigation property
    public virtual Pet Pet { get; set; } = null!;
}

public class UserPetFavorite : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid PetId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Pet Pet { get; set; } = null!;
}

public class AdoptionApplication : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid PetId { get; set; }

    [Required]
    public AdoptionApplicationStatus Status { get; set; } = AdoptionApplicationStatus.Pending;

    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? ResponseMessage { get; set; }

    public DateTime? ResponsedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Pet Pet { get; set; } = null!;
}

public enum PetType
{
    Dog = 0,
    Cat = 1,
    Bird = 2,
    Fish = 3,
    Rabbit = 4,
    Hamster = 5,
    GuineaPig = 6,
    Reptile = 7,
    Other = 8
}

public enum PetGender
{
    Male = 0,
    Female = 1,
    Unknown = 2
}

public enum PetSize
{
    ExtraSmall = 0,
    Small = 1,
    Medium = 2,
    Large = 3,
    ExtraLarge = 4
}

public enum PetStatus
{
    Available = 0,
    Pending = 1,
    Adopted = 2,
    Unavailable = 3,
    Expired = 4
}

public enum AdoptionApplicationStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Withdrawn = 3
}