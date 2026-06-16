using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Enums;

namespace lumires.Api.Features.Users.GetUserNotifications;

[UsedImplicitly]
internal sealed record Query(string Username);

[UsedImplicitly]
internal sealed record NotificationItem(
    Guid Id,
    NotificationType Type,
    string SenderId,
    string? SenderName,
    string? SenderAvatar,
    string? TargetId,
    string? TargetPayload,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);

[UsedImplicitly]
internal sealed record Response(List<NotificationItem> Notifications);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/users/{username}/notifications");
        Description(x => x.WithTags("Users"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var response = await db.GetUserNotifications(query.Username, currentUserId, ct);

        if (!response.IsSuccess)
        {
            await HttpContext.SendErrorAsync(response.Status, ct);
            return;
        }

        await Send.OkAsync(response.Value, ct);
    }
}