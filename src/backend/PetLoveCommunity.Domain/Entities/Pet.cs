using PetLoveCommunity.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.Domain.Entities;

public class Pet : BaseEntity
{
    [Required]
    [StringLength(30)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public PetType PetType { get; set; }

    [Required]
    [StringLength(50)]
    public string Breed { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Image { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public decimal Price { get; set; } = 0;

    [Required]
    [StringLength(250)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public PetGender Gender { get; set; }

    [Required]
    public AdoptionStatus AdoptionStatus { get; set; } = AdoptionStatus.Available;

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public int Views { get; set; } = 0;

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

public enum AdoptionStatus
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