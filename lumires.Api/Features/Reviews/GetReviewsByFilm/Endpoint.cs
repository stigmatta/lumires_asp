using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Enums.Common;
using lumires.Api.Features.Reviews.Common;
using lumires.Core.Abstractions.Services;
using lumires.Core.Models;

namespace lumires.Api.Features.Reviews.GetReviewsByFilm;

internal enum ContentFilterEnum
{
    All,
    LongForm,
    SpoilerFree,
    FromFriends
}

[UsedImplicitly]
internal sealed class Query
{
    public Guid Id { get; init; }
    public int FilmId { get; init; }
    public RatingEnum? Filter { get; init; } = RatingEnum.All;
    public ContentFilterEnum? Category { get; init; } = ContentFilterEnum.All;
    public ContentOrderEnum? SortBy { get; init; } = ContentOrderEnum.MostRecent;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 5;
}

internal sealed class Endpoint(DataAccess db, ICurrentUserService currentUserService, IFilmResolver filmResolver)
    : Endpoint<Query, PagedResponse<CommonReviewResponse>>
{
    public override void Configure()
    {
        Get("/films/{slug}/{filmId:int}/reviews");
        Description(x => x.WithTags("Reviews"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var userId = currentUserService.UserId;
        var wasExisting = await filmResolver.EnsureFilmExistsAsync(query.FilmId, lang, ct);

        var movieExists = await db.FilmExistsAsync(query.FilmId, ct);
        if (!movieExists)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (!wasExisting)
        {
            var emptyPaged = new PagedResponse<CommonReviewResponse>([], 0, 1, query.PageSize);
            await Send.OkAsync(emptyPaged, ct);
            return;
        }

        var response = await db.GetReviewsAsync(query, userId, ct);
        var count = await db.GetReviewsCountAsync(query, ct);

        var paged = new PagedResponse<CommonReviewResponse>(
            response,
            count,
            query.Page,
            query.PageSize
        );

        await Send.OkAsync(paged, ct);
    }
}