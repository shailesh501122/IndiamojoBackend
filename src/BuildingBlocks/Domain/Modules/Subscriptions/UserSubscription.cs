using IndiamojoBackend.BuildingBlocks.Domain.Common;

namespace IndiamojoBackend.BuildingBlocks.Domain.Modules.Subscriptions;

public sealed class UserSubscription : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid SubscriptionPlanId { get; private set; }
    public DateTime StartDateUtc { get; private set; }
    public DateTime ExpiryDateUtc { get; private set; }
    public bool IsActive { get; private set; }
    public SubscriptionPlan? SubscriptionPlan { get; private set; }

    private UserSubscription() { }

    public UserSubscription(Guid userId, Guid subscriptionPlanId, DateTime startDateUtc, DateTime expiryDateUtc)
    {
        UserId = userId;
        SubscriptionPlanId = subscriptionPlanId;
        StartDateUtc = startDateUtc;
        ExpiryDateUtc = expiryDateUtc;
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;
}
