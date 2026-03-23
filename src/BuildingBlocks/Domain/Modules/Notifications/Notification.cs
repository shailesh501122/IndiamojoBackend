using IndiamojoBackend.BuildingBlocks.Domain.Common;

namespace IndiamojoBackend.BuildingBlocks.Domain.Modules.Notifications;

public sealed class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;

    private Notification() { }

    public Notification(Guid userId, NotificationChannel channel, string subject, string message)
    {
        UserId = userId;
        Channel = channel;
        Subject = subject;
        Message = message;
    }
}
