using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetLoveCommunity.Domain.Entities;

namespace PetLoveCommunity.Infrastructure.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.BusinessName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.Description)
            .HasMaxLength(1000);

        builder.Property(v => v.Address)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(v => v.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(v => v.Website)
            .HasMaxLength(255);

        builder.Property(v => v.LogoUrl)
            .HasMaxLength(255);

        builder.Property(v => v.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.AverageRating)
            .IsRequired()
            .HasColumnType("decimal(3,2)");

        builder.Property(v => v.LicenseNumber)
            .HasMaxLength(100);

        builder.Property(v => v.OperatingHours)
            .HasMaxLength(500);

        builder.Property(v => v.ServiceArea)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(v => v.User)
            .WithOne(u => u.Vendor)
            .HasForeignKey<Vendor>(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(v => v.Type);
        builder.HasIndex(v => v.Status);
        builder.HasIndex(v => v.UserId);
        builder.HasIndex(v => v.IsVerified);
        builder.HasIndex(v => v.AverageRating);
        builder.HasIndex(v => v.CreatedAt);
    }
}

public class VendorServiceConfiguration : IEntityTypeConfiguration<VendorService>
{
    public void Configure(EntityTypeBuilder<VendorService> builder)
    {
        builder.HasKey(vs => vs.Id);

        builder.Property(vs => vs.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(vs => vs.Description)
            .HasMaxLength(1000);

        builder.Property(vs => vs.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(vs => vs.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(vs => vs.ImageUrl)
            .HasMaxLength(255);

        builder.HasOne(vs => vs.Vendor)
            .WithMany(v => v.Services)
            .HasForeignKey(vs => vs.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(vs => vs.VendorId);
        builder.HasIndex(vs => vs.Status);
        builder.HasIndex(vs => vs.Price);
    }
}

public class VendorReviewConfiguration : IEntityTypeConfiguration<VendorReview>
{
    public void Configure(EntityTypeBuilder<VendorReview> builder)
    {
        builder.HasKey(vr => vr.Id);

        builder.Property(vr => vr.Rating)
            .IsRequired();

        builder.Property(vr => vr.Comment)
            .HasMaxLength(1000);

        builder.Property(vr => vr.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.HasOne(vr => vr.User)
            .WithMany()
            .HasForeignKey(vr => vr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(vr => vr.Vendor)
            .WithMany(v => v.Reviews)
            .HasForeignKey(vr => vr.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(vr => vr.Booking)
            .WithOne(b => b.Review)
            .HasForeignKey<VendorReview>(vr => vr.BookingId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(vr => vr.UserId);
        builder.HasIndex(vr => vr.VendorId);
        builder.HasIndex(vr => vr.Rating);
        builder.HasIndex(vr => vr.Status);
        builder.HasIndex(vr => vr.CreatedAt);
    }
}

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(b => b.TotalPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.Notes)
            .HasMaxLength(500);

        builder.Property(b => b.CancellationReason)
            .HasMaxLength(500);

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Vendor)
            .WithMany(v => v.Bookings)
            .HasForeignKey(b => b.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.VendorService)
            .WithMany(vs => vs.Bookings)
            .HasForeignKey(b => b.VendorServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.UserId);
        builder.HasIndex(b => b.VendorId);
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.BookingDate);
        builder.HasIndex(b => b.StartTime);
        builder.HasIndex(b => b.CreatedAt);
    }
}

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.Value)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.MinimumOrderValue)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.HasOne(c => c.Vendor)
            .WithMany(v => v.Coupons)
            .HasForeignKey(c => c.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.HasIndex(c => c.VendorId);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.StartDate);
        builder.HasIndex(c => c.EndDate);
    }
}