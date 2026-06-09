using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Enums.Common;
using lumires.Api.Features.Reviews.Common;
using lumires.Core.Abstractions.Services;
using lumires.Core.Models;

namespace lumires.Api.Features.Reviews.GetReviews;

[UsedImplicitly]
internal enum ContentFilterEnum
{
    All,
    FromFriends,
    LongForm,
    SpoilerFree
}

[UsedImplicitly]
internal sealed class Query
{
    public RatingEnum? Filter { get; init; } = RatingEnum.All;
    public ContentFilterEnum? Category { get; init; } = ContentFilterEnum.All;
    public ContentOrderEnum? SortBy { get; init; } = ContentOrderEnum.MostRecent;
    public int? FilmId { get; init; }
    public Guid? UserId { get; init; } //if requested from user`s profile
    public Guid[]? TagIds { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 6;
}

[UsedImplicitly]
internal sealed record ReviewItemResponse(
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
) : CommonReviewResponse(Id, FilmId, UserId, Username, AvatarUrl, RepliesCount, Rating, Title, Text, LikesCount, CreatedAt,
    IsLikedByMe, IsSpoilerFree);

internal sealed class Endpoint(DataAccess db, ICurrentUserService currentUserService)
    : Endpoint<Query, PagedResponse<ReviewItemResponse>>
{
    public override void Configure()
    {
        Get("/reviews");
        Description(x => x.WithTags("Reviews"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var userId = currentUserService.UserId;

        var response = await db.GetReviewsAsync(query, lang, userId, ct);
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