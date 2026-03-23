using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Bookings;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Notifications;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Payments;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Properties;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Reviews;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Subscriptions;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Users;

namespace IndiamojoBackend.BuildingBlocks.Application.Common;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Property> Properties { get; }
    DbSet<PropertyAmenity> PropertyAmenities { get; }
    DbSet<PropertyImage> PropertyImages { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Review> Reviews { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<UserSubscription> UserSubscriptions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IJwtTokenService
{
    AuthResponse Generate(User user);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Role { get; }
}

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

public interface IPaymentGatewayService
{
    Task<string> CreatePaymentIntentAsync(decimal amount, PaymentGateway gateway, CancellationToken cancellationToken);
}

public interface INotificationService
{
    Task SendAsync(Guid userId, NotificationChannel channel, string subject, string message, CancellationToken cancellationToken);
}

public interface ISubscriptionPolicyService
{
    Task<SubscriptionEntitlement> GetEntitlementAsync(Guid userId, CancellationToken cancellationToken);
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken);
    Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken);
}

public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc, Guid UserId, string Email, string Role);
public sealed record SubscriptionEntitlement(string PlanName, int ListingLimit, bool CanFeatureListings, bool UnlimitedListings);
public sealed record PropertyResponse(Guid Id, string Title, string Description, string Type, int BHK, decimal Price, string City, decimal Latitude, decimal Longitude, string Status, bool IsFeatured, IReadOnlyCollection<string> Amenities, IReadOnlyCollection<string> Images);
public sealed record UserResponse(Guid Id, string FullName, string Email, string Role);
public sealed record BookingResponse(Guid Id, Guid PropertyId, Guid TenantId, DateTime VisitDateUtc, string Status);
public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int TotalCount);

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserResponse>()
            .ForCtorParam("Id", o => o.MapFrom(s => s.Id))
            .ForCtorParam("FullName", o => o.MapFrom(s => s.FullName))
            .ForCtorParam("Email", o => o.MapFrom(s => s.Email))
            .ForCtorParam("Role", o => o.MapFrom(s => s.Role.ToString()));

        CreateMap<Property, PropertyResponse>()
            .ForCtorParam("Id", o => o.MapFrom(s => s.Id))
            .ForCtorParam("Title", o => o.MapFrom(s => s.Title))
            .ForCtorParam("Description", o => o.MapFrom(s => s.Description))
            .ForCtorParam("Type", o => o.MapFrom(s => s.Type.ToString()))
            .ForCtorParam("BHK", o => o.MapFrom(s => s.BHK))
            .ForCtorParam("Price", o => o.MapFrom(s => s.Price))
            .ForCtorParam("City", o => o.MapFrom(s => s.Location.City))
            .ForCtorParam("Latitude", o => o.MapFrom(s => s.Location.Latitude))
            .ForCtorParam("Longitude", o => o.MapFrom(s => s.Location.Longitude))
            .ForCtorParam("Status", o => o.MapFrom(s => s.Status.ToString()))
            .ForCtorParam("IsFeatured", o => o.MapFrom(s => s.IsFeatured))
            .ForCtorParam("Amenities", o => o.MapFrom(s => s.Amenities.Select(a => a.Name).ToArray()))
            .ForCtorParam("Images", o => o.MapFrom(s => s.Images.Select(i => i.Url).ToArray()));

        CreateMap<Booking, BookingResponse>()
            .ForCtorParam("Id", o => o.MapFrom(s => s.Id))
            .ForCtorParam("PropertyId", o => o.MapFrom(s => s.PropertyId))
            .ForCtorParam("TenantId", o => o.MapFrom(s => s.TenantId))
            .ForCtorParam("VisitDateUtc", o => o.MapFrom(s => s.VisitDateUtc))
            .ForCtorParam("Status", o => o.MapFrom(s => s.Status.ToString()));
    }
}

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f is not null).ToArray();
            if (failures.Length > 0)
            {
                throw new ValidationException(failures);
            }
        }

        return await next();
    }
}

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        services.AddValidatorsFromAssembly(typeof(MappingProfile).Assembly);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MappingProfile).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}
