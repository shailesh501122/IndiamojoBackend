using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndiamojoBackend.BuildingBlocks.Application.Modules.Notifications;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Notifications;

namespace IndiamojoBackend.API.Controllers;

[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController(ISender sender) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Send(SendNotificationRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new SendNotificationCommand(request.UserId, request.Channel, request.Subject, request.Message), cancellationToken);
        return Accepted();
    }

    public sealed record SendNotificationRequest(Guid UserId, NotificationChannel Channel, string Subject, string Message);
}
