using Ardalis.Result;
using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Services;
using lumires.Core.Events.Movies;
using lumires.Core.Models;
using lumires.Domain.Exceptions;

namespace lumires.Api.Features.Reviews.GetReviewsByMovie;

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

internal enum CategoryEnum
{
    All,
    LongForm,
    SpoilerFree,
    FirstWatches,
    FromFriends
}

[UsedImplicitly]
internal sealed class Query
{
    public int MovieId { get; init; }

    public FilterEnum? Filter { get; init; } = FilterEnum.All;

    public CategoryEnum? Category { get; init; } = CategoryEnum.All;
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
    DateOnly CreatedAt,
    bool IsLikedByMe
);

internal sealed class Endpoint(DataAccess db, IMovieResolver movieResolver)
    : Endpoint<Query, PagedResponse<ReviewItemResponse>>
{
    public override void Configure()
    {
        Get("/movies/{slug}/{movieId:int}/reviews");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        
        var wasExisting = await movieResolver.EnsureMovieExistsAsync(query.MovieId, ct);

        var movieExists = await db.MovieExistsAsync(query.MovieId, ct);
        if (!movieExists)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (!wasExisting)
        {
            var emptyPaged = new PagedResponse<ReviewItemResponse>([], 0, 1, query.PageSize);
            await Send.OkAsync(emptyPaged, ct);
            return;
        }

        var response = await db.GetReviewsAsync(query, ct);
        var count = await db.GetReviewsCountAsync(query, ct);

        var paged = new PagedResponse<ReviewItemResponse>(
            response,
            count,
            query.Page,
            query.PageSize
        );

        await Send.OkAsync(paged, ct);
    }
}