using PetLoveCommunity.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.Domain.Entities;

public class Event : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public EventType Type { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [StringLength(300)]
    public string Location { get; set; } = string.Empty;

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [Required]
    public int MaxAttendees { get; set; } = 50;

    [Required]
    public decimal TicketPrice { get; set; } = 0;

    [Required]
    public EventStatus Status { get; set; } = EventStatus.Active;

    [Required]
    public bool IsPublic { get; set; } = true;

    [Required]
    public bool RequiresApproval { get; set; } = false;

    [StringLength(500)]
    public string? Requirements { get; set; }

    [StringLength(500)]
    public string? ContactInfo { get; set; }

    public DateTime? CancellationDeadline { get; set; }

    // Foreign key
    [Required]
    public Guid OrganizerId { get; set; }

    // Navigation properties
    public virtual User Organizer { get; set; } = null!;
    public virtual ICollection<EventRsvp> Rsvps { get; set; } = new List<EventRsvp>();
    public virtual ICollection<EventPhoto> Photos { get; set; } = new List<EventPhoto>();
}

public class EventRsvp : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid EventId { get; set; }

    [Required]
    public RsvpStatus Status { get; set; } = RsvpStatus.Pending;

    [Required]
    public int NumberOfAttendees { get; set; } = 1;

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime? ResponseDate { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Event Event { get; set; } = null!;
}

public class EventPhoto : BaseEntity
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
    public Guid EventId { get; set; }

    // Navigation property
    public virtual Event Event { get; set; } = null!;
}

public enum EventType
{
    Adoption = 0,
    Training = 1,
    Fundraising = 2,
    Social = 3,
    Educational = 4,
    Veterinary = 5,
    Competition = 6,
    Other = 7
}

public enum EventStatus
{
    Active = 0,
    Cancelled = 1,
    Completed = 2,
    Draft = 3
}

public enum RsvpStatus
{
    Pending = 0,
    Confirmed = 1,
    Declined = 2,
    Waitlisted = 3,
    Cancelled = 4
}