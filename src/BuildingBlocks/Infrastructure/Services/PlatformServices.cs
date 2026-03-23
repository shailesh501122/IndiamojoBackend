using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using IndiamojoBackend.BuildingBlocks.Application.Common;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Notifications;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Payments;
using IndiamojoBackend.BuildingBlocks.Infrastructure.Persistence;

namespace IndiamojoBackend.BuildingBlocks.Infrastructure.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public sealed class PaymentGatewayService : IPaymentGatewayService
{
    public Task<string> CreatePaymentIntentAsync(decimal amount, PaymentGateway gateway, CancellationToken cancellationToken)
        => Task.FromResult($"{gateway}-{Guid.NewGuid():N}");
}

public sealed class NotificationService(ApplicationDbContext context) : INotificationService
{
    public async Task SendAsync(Guid userId, NotificationChannel channel, string subject, string message, CancellationToken cancellationToken)
    {
        context.Notifications.Add(new Domain.Modules.Notifications.Notification(userId, channel, subject, message));
        await context.SaveChangesAsync(cancellationToken);
    }
}

public sealed class SubscriptionPolicyService(ApplicationDbContext context, IDateTimeProvider dateTimeProvider) : ISubscriptionPolicyService
{
    public async Task<SubscriptionEntitlement> GetEntitlementAsync(Guid userId, CancellationToken cancellationToken)
    {
        var subscription = await context.UserSubscriptions
            .Include(x => x.SubscriptionPlan)
            .Where(x => x.UserId == userId && x.IsActive && x.ExpiryDateUtc >= dateTimeProvider.UtcNow)
            .OrderByDescending(x => x.ExpiryDateUtc)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Active subscription not found.");

        var plan = subscription.SubscriptionPlan ?? throw new InvalidOperationException("Plan not available.");
        return new SubscriptionEntitlement(plan.Name, plan.ListingLimit, plan.IsFeaturedEnabled, plan.ListingLimit <= 0);
    }
}

public sealed class CacheService(IDistributedCache distributedCache) : ICacheService
{
    private static readonly ConcurrentDictionary<string, object> Fallback = new();

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        var payload = await distributedCache.GetStringAsync(key, cancellationToken);
        if (!string.IsNullOrWhiteSpace(payload))
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(payload);
        }

        return Fallback.TryGetValue(key, out var value) ? (T?)value : default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken)
    {
        var payload = System.Text.Json.JsonSerializer.Serialize(value);
        await distributedCache.SetStringAsync(key, payload, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry }, cancellationToken);
        Fallback[key] = value!;
    }
}
