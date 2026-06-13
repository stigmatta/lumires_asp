using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Models;

namespace lumires.Api.Features.Reviews.GetCommentReplies;

[UsedImplicitly]
internal sealed class Query
{
    public Guid ReviewId { get; init; }
    public Guid ReplyId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 6;
}

[UsedImplicitly]
internal sealed record CommentItemResponse(
    Guid Id,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    string Text,
    int LikesCount,
    int RepliesCount,
    bool IsLikedByMe,
    bool IsSpoilerFree,
    DateTime CreatedAt,
    Guid? ParentCommentId,
    Guid? TargetedUserId,
    string? TargetedUserUsername);

internal sealed class Endpoint(DataAccess db)
    : Endpoint<Query, PagedResponse<CommentItemResponse>>
{
    public override void Configure()
    {
        Get("/films/{filmId}/reviews/{reviewId}/replies/{replyId}/replies");
        Description(x => x.WithTags("Reviews"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var response = await db.GetRepliesByCommentId(query, ct);
        var count = await db.GetRepliesCountAsync(query, ct);

        var paged = new PagedResponse<CommentItemResponse>(
            response,
            count,
            query.Page,
            query.PageSize
        );

        await Send.OkAsync(paged, ct);
    }
}
