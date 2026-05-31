using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Features.Reviews.Common;
using lumires.Core.Abstractions.Services;
using lumires.Core.Models;

namespace lumires.Api.Features.Reviews.GetRecentReviews;

[UsedImplicitly]
internal sealed class Query
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 4;
}

[UsedImplicitly]
internal sealed record RecentReviewItem(
    Guid Id,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    int RepliesCount,
    float? Rating,
    string? Title,
    string Text,
    int LikesCount,
    DateTime CreatedAt,
    bool IsLikedByMe,
    bool IsSpoilerFree,
    int FilmId,
    string FilmTitle,
    string FilmSlug,
    string? FilmPosterPath
) : CommonReviewResponse(Id, UserId, Username, AvatarUrl, RepliesCount, Rating, Title, Text, LikesCount, CreatedAt,
    IsLikedByMe, IsSpoilerFree);

internal sealed class Endpoint(DataAccess db, ICurrentUserService currentUserService)
    : Endpoint<Query, PagedResponse<RecentReviewItem>>
{
    public override void Configure()
    {
        Get("/reviews/recent");
        Description(x => x.WithTags("Reviews"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var userId = currentUserService.UserId;

        var response = await db.GetRecentReviewsAsync(query, lang, userId, ct);
        var count = await db.GetReviewsCountAsync(ct);

        var paged = new PagedResponse<RecentReviewItem>(
            response,
            count,
            query.Page,
            query.PageSize
        );

        await Send.OkAsync(paged, ct);
    }
}