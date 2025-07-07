using PetLoveCommunity.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.Domain.Entities;

public class Vendor : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string BusinessName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Website { get; set; }

    [StringLength(255)]
    public string? LogoUrl { get; set; }

    [Required]
    public VendorType Type { get; set; }

    [Required]
    public VendorStatus Status { get; set; } = VendorStatus.Pending;

    [Required]
    public bool IsVerified { get; set; } = false;

    [Required]
    public decimal AverageRating { get; set; } = 0;

    [Required]
    public int TotalReviews { get; set; } = 0;

    [StringLength(100)]
    public string? LicenseNumber { get; set; }

    public DateTime? VerifiedAt { get; set; }

    [StringLength(500)]
    public string? OperatingHours { get; set; }

    [StringLength(100)]
    public string? ServiceArea { get; set; }

    // Foreign key
    [Required]
    public Guid UserId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<VendorService> Services { get; set; } = new List<VendorService>();
    public virtual ICollection<VendorReview> Reviews { get; set; } = new List<VendorReview>();
    public virtual ICollection<VendorPhoto> Photos { get; set; } = new List<VendorPhoto>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
}

public class VendorService : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int DurationMinutes { get; set; }

    [Required]
    public ServiceStatus Status { get; set; } = ServiceStatus.Active;

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    // Foreign key
    [Required]
    public Guid VendorId { get; set; }

    // Navigation properties
    public virtual Vendor Vendor { get; set; } = null!;
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

public class VendorReview : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid VendorId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }

    [Required]
    public ReviewStatus Status { get; set; } = ReviewStatus.Active;

    public Guid? BookingId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Vendor Vendor { get; set; } = null!;
    public virtual Booking? Booking { get; set; }
}

public class VendorPhoto : BaseEntity
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
    public Guid VendorId { get; set; }

    // Navigation property
    public virtual Vendor Vendor { get; set; } = null!;
}

public class Booking : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid VendorId { get; set; }

    [Required]
    public Guid VendorServiceId { get; set; }

    [Required]
    public DateTime BookingDate { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    [Required]
    public decimal TotalPrice { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(500)]
    public string? CancellationReason { get; set; }

    public DateTime? CancelledAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Vendor Vendor { get; set; } = null!;
    public virtual VendorService VendorService { get; set; } = null!;
    public virtual VendorReview? Review { get; set; }
}

public class Coupon : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public CouponType Type { get; set; }

    [Required]
    public decimal Value { get; set; }

    [Required]
    public decimal MinimumOrderValue { get; set; } = 0;

    [Required]
    public int MaxUsages { get; set; } = 1;

    [Required]
    public int CurrentUsages { get; set; } = 0;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public CouponStatus Status { get; set; } = CouponStatus.Active;

    // Foreign key
    [Required]
    public Guid VendorId { get; set; }

    // Navigation properties
    public virtual Vendor Vendor { get; set; } = null!;
    public virtual ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();
}

public class CouponUsage : BaseEntity
{
    [Required]
    public Guid CouponId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid BookingId { get; set; }

    [Required]
    public decimal DiscountAmount { get; set; }

    // Navigation properties
    public virtual Coupon Coupon { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Booking Booking { get; set; } = null!;
}

public enum VendorType
{
    Veterinarian = 0,
    Groomer = 1,
    Trainer = 2,
    PetSitter = 3,
    PetStore = 4,
    Shelter = 5,
    Other = 6
}

public enum VendorStatus
{
    Pending = 0,
    Active = 1,
    Suspended = 2,
    Rejected = 3
}

public enum ServiceStatus
{
    Active = 0,
    Inactive = 1,
    Suspended = 2
}

public enum ReviewStatus
{
    Active = 0,
    Hidden = 1,
    Flagged = 2
}

public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5
}

public enum CouponType
{
    Percentage = 0,
    FixedAmount = 1
}

public enum CouponStatus
{
    Active = 0,
    Inactive = 1,
    Expired = 2
}