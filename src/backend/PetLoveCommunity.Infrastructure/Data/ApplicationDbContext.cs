using Microsoft.EntityFrameworkCore;
using PetLoveCommunity.Domain.Entities;
using System.Linq.Expressions;

namespace PetLoveCommunity.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // User and Authentication
    public DbSet<User> Users { get; set; }

    // Pet Adoption
    public DbSet<Pet> Pets { get; set; }
    public DbSet<PetPhoto> PetPhotos { get; set; }
    public DbSet<UserPetFavorite> UserPetFavorites { get; set; }
    public DbSet<AdoptionApplication> AdoptionApplications { get; set; }

    // Events
    public DbSet<Event> Events { get; set; }
    public DbSet<EventRsvp> EventRsvps { get; set; }
    public DbSet<EventPhoto> EventPhotos { get; set; }

    // Vendors and Marketplace
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<VendorService> VendorServices { get; set; }
    public DbSet<VendorReview> VendorReviews { get; set; }
    public DbSet<VendorPhoto> VendorPhotos { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<CouponUsage> CouponUsages { get; set; }

    // Social Features
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostComment> PostComments { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<PostCommentLike> PostCommentLikes { get; set; }
    public DbSet<PostShare> PostShares { get; set; }
    public DbSet<PostPhoto> PostPhotos { get; set; }
    public DbSet<UserFollowing> UserFollowings { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure table names
        ConfigureTableNames(modelBuilder);

        // Configure relationships
        ConfigureRelationships(modelBuilder);

        // Configure indexes
        ConfigureIndexes(modelBuilder);

        // Configure soft delete
        ConfigureSoftDelete(modelBuilder);
    }

    private void ConfigureTableNames(ModelBuilder modelBuilder)
    {
        // User and Authentication
        modelBuilder.Entity<User>().ToTable("Users");

        // Pet Adoption
        modelBuilder.Entity<Pet>().ToTable("Pets");
        modelBuilder.Entity<PetPhoto>().ToTable("PetPhotos");
        modelBuilder.Entity<UserPetFavorite>().ToTable("UserPetFavorites");
        modelBuilder.Entity<AdoptionApplication>().ToTable("AdoptionApplications");

        // Events
        modelBuilder.Entity<Event>().ToTable("Events");
        modelBuilder.Entity<EventRsvp>().ToTable("EventRsvps");
        modelBuilder.Entity<EventPhoto>().ToTable("EventPhotos");

        // Vendors and Marketplace
        modelBuilder.Entity<Vendor>().ToTable("Vendors");
        modelBuilder.Entity<VendorService>().ToTable("VendorServices");
        modelBuilder.Entity<VendorReview>().ToTable("VendorReviews");
        modelBuilder.Entity<VendorPhoto>().ToTable("VendorPhotos");
        modelBuilder.Entity<Booking>().ToTable("Bookings");
        modelBuilder.Entity<Coupon>().ToTable("Coupons");
        modelBuilder.Entity<CouponUsage>().ToTable("CouponUsages");

        // Social Features
        modelBuilder.Entity<Post>().ToTable("Posts");
        modelBuilder.Entity<PostComment>().ToTable("PostComments");
        modelBuilder.Entity<PostLike>().ToTable("PostLikes");
        modelBuilder.Entity<PostCommentLike>().ToTable("PostCommentLikes");
        modelBuilder.Entity<PostShare>().ToTable("PostShares");
        modelBuilder.Entity<PostPhoto>().ToTable("PostPhotos");
        modelBuilder.Entity<UserFollowing>().ToTable("UserFollowings");
        modelBuilder.Entity<Notification>().ToTable("Notifications");
    }

    private void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        // User relationships
        modelBuilder.Entity<Pet>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.Pets)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Pet>()
            .HasOne(p => p.AdoptedBy)
            .WithMany()
            .HasForeignKey(p => p.AdoptedById)
            .OnDelete(DeleteBehavior.SetNull);

        // User-Pet favorites (many-to-many)
        modelBuilder.Entity<UserPetFavorite>()
            .HasKey(upf => new { upf.UserId, upf.PetId });

        modelBuilder.Entity<UserPetFavorite>()
            .HasOne(upf => upf.User)
            .WithMany(u => u.FavoritePets)
            .HasForeignKey(upf => upf.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserPetFavorite>()
            .HasOne(upf => upf.Pet)
            .WithMany(p => p.FavoriteByUsers)
            .HasForeignKey(upf => upf.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Event relationships
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organizer)
            .WithMany(u => u.Events)
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EventRsvp>()
            .HasKey(er => new { er.UserId, er.EventId });

        // Vendor relationships
        modelBuilder.Entity<Vendor>()
            .HasOne(v => v.User)
            .WithOne(u => u.Vendor)
            .HasForeignKey<Vendor>(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Post relationships
        modelBuilder.Entity<Post>()
            .HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Post likes (many-to-many)
        modelBuilder.Entity<PostLike>()
            .HasKey(pl => new { pl.UserId, pl.PostId });

        // User following (many-to-many)
        modelBuilder.Entity<UserFollowing>()
            .HasKey(uf => new { uf.FollowerId, uf.FollowingId });

        modelBuilder.Entity<UserFollowing>()
            .HasOne(uf => uf.Follower)
            .WithMany()
            .HasForeignKey(uf => uf.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserFollowing>()
            .HasOne(uf => uf.Following)
            .WithMany()
            .HasForeignKey(uf => uf.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent cascade delete loops
        modelBuilder.Entity<PostComment>()
            .HasOne(pc => pc.ParentComment)
            .WithMany(pc => pc.Replies)
            .HasForeignKey(pc => pc.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // User indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Role);

        // Pet indexes
        modelBuilder.Entity<Pet>()
            .HasIndex(p => p.Status);

        modelBuilder.Entity<Pet>()
            .HasIndex(p => p.Type);

        modelBuilder.Entity<Pet>()
            .HasIndex(p => p.OwnerId);

        // Event indexes
        modelBuilder.Entity<Event>()
            .HasIndex(e => e.StartDate);

        modelBuilder.Entity<Event>()
            .HasIndex(e => e.Type);

        modelBuilder.Entity<Event>()
            .HasIndex(e => e.OrganizerId);

        // Vendor indexes
        modelBuilder.Entity<Vendor>()
            .HasIndex(v => v.Type);

        modelBuilder.Entity<Vendor>()
            .HasIndex(v => v.Status);

        modelBuilder.Entity<Vendor>()
            .HasIndex(v => v.UserId);

        // Post indexes
        modelBuilder.Entity<Post>()
            .HasIndex(p => p.UserId);

        modelBuilder.Entity<Post>()
            .HasIndex(p => p.CreatedAt);

        modelBuilder.Entity<Post>()
            .HasIndex(p => p.Status);

        // Notification indexes
        modelBuilder.Entity<Notification>()
            .HasIndex(n => n.UserId);

        modelBuilder.Entity<Notification>()
            .HasIndex(n => n.IsRead);

        modelBuilder.Entity<Notification>()
            .HasIndex(n => n.CreatedAt);
    }

    private void ConfigureSoftDelete(ModelBuilder modelBuilder)
    {
        // Configure soft delete for all entities
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(Domain.Common.BaseEntity).IsAssignableFrom(e.ClrType));

        foreach (var entityType in entityTypes)
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var propertyMethodInfo = typeof(EF).GetMethod(nameof(EF.Property))!.MakeGenericMethod(typeof(bool));
            var isDeletedProperty = Expression.Call(propertyMethodInfo, parameter, Expression.Constant("IsDeleted"));
            var compareExpression = Expression.MakeBinary(ExpressionType.Equal, isDeletedProperty, Expression.Constant(false));
            var lambda = Expression.Lambda(compareExpression, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Domain.Common.BaseEntity)
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var entity = (Domain.Common.BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}