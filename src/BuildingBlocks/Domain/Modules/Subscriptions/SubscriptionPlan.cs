using IndiamojoBackend.BuildingBlocks.Domain.Common;

namespace IndiamojoBackend.BuildingBlocks.Domain.Modules.Subscriptions;

public sealed class SubscriptionPlan : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public SubscriptionType Type { get; private set; }
    public decimal Price { get; private set; }
    public int ListingLimit { get; private set; }
    public bool IsFeaturedEnabled { get; private set; }

    private SubscriptionPlan() { }

    public SubscriptionPlan(string name, SubscriptionType type, decimal price, int listingLimit, bool isFeaturedEnabled)
    {
        Name = name;
        Type = type;
        Price = price;
        ListingLimit = listingLimit;
        IsFeaturedEnabled = isFeaturedEnabled;
    }
}
