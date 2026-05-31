using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Threads.GetThread;

[UsedImplicitly]
internal sealed record Query(Guid ThreadId);

[UsedImplicitly]
internal sealed record Response(
    Guid Id,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    int RepliesCount,
    string? Title,
    string Text,
    int LikesCount,
    DateOnly CreatedAt,
    bool IsLikedByMe,
    IEnumerable<CommentItemResponse> Comments);

[UsedImplicitly]
internal sealed record CommentItemResponse(
    Guid Id,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    int LikesCount,
    bool IsLikedByMe,
    string Text,
    DateOnly CreatedAt,
    Guid? TargetedUserId,
    string? TargetedUserUsername);

internal sealed class Endpoint(DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/threads/{threadId}");
        Description(x => x.WithTags("Threads"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var response = await db.GetThreadByIdAsync(query, ct);

        if (response is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(response, ct);
    }
}