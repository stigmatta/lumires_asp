using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Enums.Common;
using lumires.Api.Features.Films.Contracts;
using lumires.Core.Abstractions.Services;
using lumires.Core.Models;

namespace lumires.Api.Features.Films.GetUserWatchlist;


[UsedImplicitly]
internal sealed class Query
{
    public string Username { get; init; } = null!;
    public RatingEnum? Rating { get; init; } = RatingEnum.All;
    public string[]? Genres { get; init; } = [];
    public FilmContentOrder? SortBy { get; init; } = FilmContentOrder.MostRecent;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 16;
}

internal sealed class Endpoint(DataAccess db, ICurrentUserService currentUserService)
    : Endpoint<Query, PagedResponse<CommonFilmListResponse>>
{
    public override void Configure()
    {
        Get("/user/{username}/watchlist");
        Description(x => x.WithTags("Films"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var currentUserId = currentUserService.UserId;

        var items = await db.GetWatchlistByUser(query, lang, currentUserId, ct);
        var count = await db.GetFilmsCountAsync(query, lang, currentUserId, ct);

        var paged = new PagedResponse<CommonFilmListResponse>(
            items,
            count,
            query.Page,
            query.PageSize
        );

        await Send.OkAsync(paged, ct);
    }
}