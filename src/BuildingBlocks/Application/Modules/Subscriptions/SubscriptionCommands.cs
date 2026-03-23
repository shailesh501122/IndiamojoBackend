using MediatR;
using Microsoft.EntityFrameworkCore;
using IndiamojoBackend.BuildingBlocks.Application.Common;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Subscriptions;

namespace IndiamojoBackend.BuildingBlocks.Application.Modules.Subscriptions;

public sealed record SubscribeUserCommand(Guid UserId, Guid PlanId, int DurationInMonths) : IRequest<Guid>;
public sealed record GetCurrentSubscriptionQuery(Guid UserId) : IRequest<SubscriptionEntitlement>;

public sealed class SubscribeUserHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider) : IRequestHandler<SubscribeUserCommand, Guid>
{
    public async Task<Guid> Handle(SubscribeUserCommand request, CancellationToken cancellationToken)
    {
        var current = await context.UserSubscriptions.Where(x => x.UserId == request.UserId && x.IsActive).ToListAsync(cancellationToken);
        foreach (var subscription in current)
        {
            subscription.Deactivate();
        }

        var entity = new UserSubscription(request.UserId, request.PlanId, dateTimeProvider.UtcNow, dateTimeProvider.UtcNow.AddMonths(request.DurationInMonths));
        context.UserSubscriptions.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}

public sealed class GetCurrentSubscriptionHandler(ISubscriptionPolicyService subscriptionPolicyService) : IRequestHandler<GetCurrentSubscriptionQuery, SubscriptionEntitlement>
{
    public Task<SubscriptionEntitlement> Handle(GetCurrentSubscriptionQuery request, CancellationToken cancellationToken)
        => subscriptionPolicyService.GetEntitlementAsync(request.UserId, cancellationToken);
}
