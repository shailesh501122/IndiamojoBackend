using Microsoft.EntityFrameworkCore;
using IndiamojoBackend.BuildingBlocks.Application.Common;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Bookings;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Notifications;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Payments;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Properties;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Reviews;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Subscriptions;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Users;

namespace IndiamojoBackend.BuildingBlocks.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyAmenity> PropertyAmenities => Set<PropertyAmenity>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(255);
            entity.Property(x => x.FullName).HasMaxLength(120);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasIndex(x => x.Token).IsUnique();
            entity.HasOne(x => x.User).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<Property>(entity =>
        {
            entity.ToTable("properties");
            entity.HasIndex(x => x.Price);
            entity.HasIndex(x => x.Type);
            entity.OwnsOne(x => x.Location, owned =>
            {
                owned.Property(x => x.City).HasColumnName("city").HasMaxLength(100);
                owned.Property(x => x.Latitude).HasColumnName("latitude");
                owned.Property(x => x.Longitude).HasColumnName("longitude");
                owned.HasIndex(x => x.City);
            });
            entity.HasMany(x => x.Amenities).WithOne().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Images).WithOne().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PropertyAmenity>(entity =>
        {
            entity.ToTable("property_amenities");
            entity.Property(x => x.Name).HasMaxLength(120);
        });

        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.ToTable("property_images");
            entity.Property(x => x.Url).HasMaxLength(1000);
        });

        modelBuilder.Entity<Booking>().ToTable("bookings");
        modelBuilder.Entity<Payment>().ToTable("payments");
        modelBuilder.Entity<Review>().ToTable("reviews");
        modelBuilder.Entity<Notification>().ToTable("notifications");
        modelBuilder.Entity<SubscriptionPlan>().ToTable("subscription_plans");
        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.ToTable("user_subscriptions");
            entity.HasOne(x => x.SubscriptionPlan).WithMany().HasForeignKey(x => x.SubscriptionPlanId);
            entity.HasIndex(x => new { x.UserId, x.IsActive });
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is IndiamojoBackend.BuildingBlocks.Domain.Common.BaseEntity baseEntity && entry.State == EntityState.Modified)
            {
                baseEntity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
