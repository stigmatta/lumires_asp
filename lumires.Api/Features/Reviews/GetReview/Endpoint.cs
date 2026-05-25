using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetReview;

[UsedImplicitly]
internal sealed record Query(Guid ReviewId);

[UsedImplicitly]
internal sealed record Response(
    Guid Id,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    int RepliesCount,
    float? Rating,
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
    DateOnly CreatedAt,
    Guid? TargetedUserId,
    string? TargetedUserUsername);

internal sealed class Endpoint(DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/films/{slug}/{filmId}/reviews/{reviewId}");
        Description(x => x.WithTags("Reviews"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var response = await db.GetReviewByIdAsync(query, ct);

        if (response is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(response, ct);
    }
}