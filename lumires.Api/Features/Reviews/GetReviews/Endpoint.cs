using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Models;

namespace lumires.Api.Features.Reviews.GetReviews;

internal enum FilterEnum
{
    All,
    FiveStars,
    FourStars,
    ThreeStars,
    UnderThree
}

internal enum SortEnum
{
    MostRecent,
    MostLiked,
    MostReplies,
    HighestRated
}

[UsedImplicitly]
internal sealed class Query
{
    public Guid MovieId { get; init; }

    public FilterEnum? Filter { get; init; } = FilterEnum.All;

    public SortEnum? SortBy { get; init; } = SortEnum.MostRecent;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 5;
}

[UsedImplicitly]
internal sealed record ReviewItemResponse(
    Guid Id,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    int RepliesCount,
    decimal? Rating,
    string? Title,
    string Text,
    int LikesCount,
    DateOnly CreatedAt
);

internal sealed class Endpoint(DataAccess db)
    : Endpoint<Query, PagedResponse<ReviewItemResponse>>
{
    public override void Configure()
    {
        Get("/movies/{slug}/{movieId}/reviews");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var response = await db.GetReviewsAsync(query, ct);
        var count = await db.GetReviewsCountAsync(ct);

        var paged = new PagedResponse<ReviewItemResponse>(response, count, query.Page, query.PageSize);

        await Send.OkAsync(paged, ct);
    }
}