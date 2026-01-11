using FastEndpoints;
using lumires.Api.Features.Notifications;
using lumires.Api.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace lumires.Api;

public record SignalRTestRequest
{
    public string TargetUserId { get; init; } = null!;
}

public class SignalRTestEndpoint(IHubContext<NotificationHub, INotificationClient> hubContext)
    : Endpoint<SignalRTestRequest>
{
    public override void Configure()
    {
        Post("/api/test/signalr-send-anonymous");
        AllowAnonymous(); 
    }

    public override async Task HandleAsync(SignalRTestRequest req, CancellationToken ct)
    {
        var command = new NotificationCommand(
            Type: EventTypes.LikedReview, 
            SenderId: req.TargetUserId,
            TargetId: req.TargetUserId,
            CreatedAt: DateTime.UtcNow
        );
        
        await hubContext.Clients
            .User(req.TargetUserId)
            .ReceiveNotification(command);

        await Send.OkAsync(new { Message = $"Attempted to send message to {req.TargetUserId}" }, ct);
    }
}