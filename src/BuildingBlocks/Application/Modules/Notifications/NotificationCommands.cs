using MediatR;
using IndiamojoBackend.BuildingBlocks.Application.Common;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Notifications;

namespace IndiamojoBackend.BuildingBlocks.Application.Modules.Notifications;

public sealed record SendNotificationCommand(Guid UserId, NotificationChannel Channel, string Subject, string Message) : IRequest;

public sealed class SendNotificationHandler(INotificationService notificationService) : IRequestHandler<SendNotificationCommand>
{
    public Task Handle(SendNotificationCommand request, CancellationToken cancellationToken)
        => notificationService.SendAsync(request.UserId, request.Channel, request.Subject, request.Message, cancellationToken);
}
