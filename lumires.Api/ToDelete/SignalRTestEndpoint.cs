using Contracts.Abstractions;
using Contracts.Messaging;
using FastEndpoints;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using lumires.Domain.Persistence;

namespace lumires.Api.ToDelete;

internal class SignalRTestEndpoint(
    INotificationService notificationService,
    ICurrentUserService currentUserService,
    AppDbContext db
)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/signalr/send-anonymous");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var notification = new UserNotification
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Type = NotificationType.Followed,
            SenderId = currentUserId.ToString(),
            TargetId = currentUserId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        db.UserNotifications.Add(notification);
        await db.SaveChangesAsync(ct);

        var command = new NotificationMessage(
            notification.Type,
            notification.SenderId,
            notification.TargetId,
            notification.CreatedAt
        );

        await notificationService.SendToUserAsync(currentUserId, command);

        await Send.NoContentAsync(ct);
    }
}