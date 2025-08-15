using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetLoveCommunity.Domain.Entities;

namespace PetLoveCommunity.Infrastructure.Configurations;

public class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(p => p.PetType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Breed)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Image)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(p => p.DateOfBirth)
            .IsRequired();

        builder.Property(p => p.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(p => p.Gender)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.AdoptionStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.Views)
            .IsRequired();

        // Relationships
        builder.HasOne(p => p.Owner)
            .WithMany(u => u.Pets)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.AdoptedBy)
            .WithMany()
            .HasForeignKey(p => p.AdoptedById)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(p => p.AdoptionStatus);
        builder.HasIndex(p => p.PetType);
        builder.HasIndex(p => p.OwnerId);
        builder.HasIndex(p => p.AdoptedById);
        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => p.Views);
    }
}

public class PetPhotoConfiguration : IEntityTypeConfiguration<PetPhoto>
{
    public void Configure(EntityTypeBuilder<PetPhoto> builder)
    {
        builder.HasKey(pp => pp.Id);

        builder.Property(pp => pp.PhotoUrl)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(pp => pp.Caption)
            .HasMaxLength(255);

        builder.HasOne(pp => pp.Pet)
            .WithMany(p => p.Photos)
            .HasForeignKey(pp => pp.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pp => pp.PetId);
        builder.HasIndex(pp => pp.DisplayOrder);
        builder.HasIndex(pp => pp.IsPrimary);
    }
}

public class UserPetFavoriteConfiguration : IEntityTypeConfiguration<UserPetFavorite>
{
    public void Configure(EntityTypeBuilder<UserPetFavorite> builder)
    {
        builder.HasKey(upf => upf.Id);

        builder.HasOne(upf => upf.User)
            .WithMany(u => u.FavoritePets)
            .HasForeignKey(upf => upf.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(upf => upf.Pet)
            .WithMany(p => p.FavoriteByUsers)
            .HasForeignKey(upf => upf.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(upf => new { upf.UserId, upf.PetId })
            .IsUnique();
    }
}

public class AdoptionApplicationConfiguration : IEntityTypeConfiguration<AdoptionApplication>
{
    public void Configure(EntityTypeBuilder<AdoptionApplication> builder)
    {
        builder.HasKey(aa => aa.Id);

        builder.Property(aa => aa.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(aa => aa.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(aa => aa.ResponseMessage)
            .HasMaxLength(1000);

        builder.HasOne(aa => aa.User)
            .WithMany(u => u.AdoptionApplications)
            .HasForeignKey(aa => aa.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(aa => aa.Pet)
            .WithMany(p => p.AdoptionApplications)
            .HasForeignKey(aa => aa.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(aa => aa.UserId);
        builder.HasIndex(aa => aa.PetId);
        builder.HasIndex(aa => aa.Status);
        builder.HasIndex(aa => aa.CreatedAt);
    }
}