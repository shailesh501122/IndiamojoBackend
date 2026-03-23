using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndiamojoBackend.BuildingBlocks.Application.Modules.Subscriptions;

namespace IndiamojoBackend.API.Controllers;

[ApiController]
[Route("api/subscriptions")]
public sealed class SubscriptionsController(ISender sender) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Subscribe(SubscribeRequest request, CancellationToken cancellationToken)
        => Ok(new { subscriptionId = await sender.Send(new SubscribeUserCommand(request.UserId, request.PlanId, request.DurationInMonths), cancellationToken) });

    [Authorize]
    [HttpGet("{userId:guid}/current")]
    public async Task<IActionResult> GetCurrent(Guid userId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetCurrentSubscriptionQuery(userId), cancellationToken));

    public sealed record SubscribeRequest(Guid UserId, Guid PlanId, int DurationInMonths);
}
