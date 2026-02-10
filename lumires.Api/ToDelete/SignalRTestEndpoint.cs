using Core.Abstractions.Data;
using Core.Abstractions.Services;
using Core.Messaging;
using Domain.Entities;
using Domain.Enums;
using FastEndpoints;

namespace Api.ToDelete;

internal class SignalRTestEndpoint(
    INotificationService notificationService,
    ICurrentUserService currentUserService,
    IAppDbContext db
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