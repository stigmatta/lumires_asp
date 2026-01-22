using FastEndpoints;
using lumires.Api.Domain.Entities;
using lumires.Api.Infrastructure.Hubs;
using lumires.Api.Infrastructure.Persistence;
using lumires.Api.Shared.Abstractions;
using lumires.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace lumires.Api.ToDelete;

public class SignalRTestEndpoint(
    IHubContext<NotificationHub, INotificationClient> hubContext,
    ICurrentUserService currentUserService,
    AppDbContext db
    )
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/api/test/signalr-send-anonymous");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var notification = new UserNotification
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Type = EventTypes.Followed,
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

        await hubContext.Clients
            .User(currentUserId.ToString())
            .ReceiveNotification(command);

        await Send.NoContentAsync(ct);
    }
}