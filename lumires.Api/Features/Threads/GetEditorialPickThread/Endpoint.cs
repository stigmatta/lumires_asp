using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Threads.GetEditorialPickThread;

[UsedImplicitly]
internal sealed record ThreadCommentPreview(
    Guid Id,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    string Text,
    int LikesCount,
    bool IsLikedByMe,
    DateTime CreatedAt);

[UsedImplicitly]
internal sealed record Response(
    Guid Id,
    string? Title,
    string Text,
    Guid UserId,
    string Username,
    DateTime CreatedAt,
    int ReplyCount,
    int LikesCount,
    IReadOnlyCollection<ThreadCommentPreview> Comments
);

internal sealed class Endpoint(DataAccess db, ICurrentUserService currentUserService)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/threads/editorial");
        Description(x => x.WithTags("Threads"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;

        var response = await db.GetEditorialThreadAsync(currentUserId, ct);

        if (response is null)
        {
            await Send.NoContentAsync(ct);
            return;
        }

        await Send.OkAsync(response, ct);
    }
}