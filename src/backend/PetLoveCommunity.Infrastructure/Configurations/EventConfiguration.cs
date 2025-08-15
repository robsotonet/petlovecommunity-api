using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetLoveCommunity.Domain.Entities;

namespace PetLoveCommunity.Infrastructure.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Location)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(255);

        builder.Property(e => e.TicketPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Requirements)
            .HasMaxLength(500);

        builder.Property(e => e.ContactInfo)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(e => e.Organizer)
            .WithMany(u => u.Events)
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.StartDate);
        builder.HasIndex(e => e.EndDate);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.OrganizerId);
        builder.HasIndex(e => e.IsPublic);
        builder.HasIndex(e => e.CreatedAt);
    }
}

public class EventRsvpConfiguration : IEntityTypeConfiguration<EventRsvp>
{
    public void Configure(EntityTypeBuilder<EventRsvp> builder)
    {
        builder.HasKey(er => er.Id);

        builder.Property(er => er.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(er => er.Notes)
            .HasMaxLength(500);

        builder.HasOne(er => er.User)
            .WithMany(u => u.EventRsvps)
            .HasForeignKey(er => er.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(er => er.Event)
            .WithMany(e => e.Rsvps)
            .HasForeignKey(er => er.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(er => new { er.UserId, er.EventId })
            .IsUnique();

        builder.HasIndex(er => er.Status);
        builder.HasIndex(er => er.CreatedAt);
    }
}

public class EventPhotoConfiguration : IEntityTypeConfiguration<EventPhoto>
{
    public void Configure(EntityTypeBuilder<EventPhoto> builder)
    {
        builder.HasKey(ep => ep.Id);

        builder.Property(ep => ep.PhotoUrl)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ep => ep.Caption)
            .HasMaxLength(255);

        builder.HasOne(ep => ep.Event)
            .WithMany(e => e.Photos)
            .HasForeignKey(ep => ep.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ep => ep.EventId);
        builder.HasIndex(ep => ep.DisplayOrder);
        builder.HasIndex(ep => ep.IsPrimary);
    }
}