using PetLoveCommunity.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.Domain.Entities;

public class User : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }

    [StringLength(255)]
    public string? ProfilePictureUrl { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.Free;

    [Required]
    public UserStatus Status { get; set; } = UserStatus.Active;

    public bool IsEmailVerified { get; set; } = false;

    public DateTime? LastLoginAt { get; set; }

    public DateTime? EmailVerifiedAt { get; set; }

    [StringLength(255)]
    public string? ResetPasswordToken { get; set; }

    public DateTime? ResetPasswordTokenExpiry { get; set; }

    [StringLength(255)]
    public string? EmailVerificationToken { get; set; }

    public DateTime? EmailVerificationTokenExpiry { get; set; }

    // Navigation properties
    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual ICollection<UserPetFavorite> FavoritePets { get; set; } = new List<UserPetFavorite>();
    public virtual ICollection<EventRsvp> EventRsvps { get; set; } = new List<EventRsvp>();
    public virtual ICollection<AdoptionApplication> AdoptionApplications { get; set; } = new List<AdoptionApplication>();
    public virtual Vendor? Vendor { get; set; }
}

public enum UserRole
{
    Free = 0,
    Premium = 1,
    Vendor = 2,
    Shelter = 3,
    Admin = 4
}

public enum UserStatus
{
    Active = 0,
    Inactive = 1,
    Suspended = 2,
    Banned = 3
}