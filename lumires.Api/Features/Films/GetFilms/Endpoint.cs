using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Enums.Common;
using lumires.Core.Abstractions.Services;
using lumires.Core.Models;

namespace lumires.Api.Features.Films.GetFilms;

internal enum FilmContentFilter
{
    All,
    Popular,
    TopRated,
    NewReleases,
    FirstWatches,
    HiddenGems
}

internal enum FilmContentOrder
{
    MostRecent,
    MostLiked,
    MostReplies,
    HighestRated,
    LeastRated
}

[UsedImplicitly]
internal sealed class Query
{
    public RatingEnum? Rating { get; init; } = RatingEnum.All;
    public FilmContentFilter? Content { get; init; } = FilmContentFilter.All;
    public string[]? Genres { get; init; } = [];
    public FilmContentOrder? SortBy { get; init; } = FilmContentOrder.MostRecent;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 16;
}

[UsedImplicitly]
internal sealed record FilmItemResponse(
    int Id,
    string Title,
    int? ReleaseYear,
    string[] Genres,
    float VoteAverage,
    string? PosterPath
);

internal sealed class Endpoint(IGetFilms db, ICurrentUserService currentUserService)
    : Endpoint<Query, PagedResponse<FilmItemResponse>>
{
    public override void Configure()
    {
        Get("/films/");
        Description(x => x.WithTags("Films"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;

        var items = await db.GetFilmsAsync(query, lang, ct);
        var count = await db.GetFilmsCountAsync(query, lang, ct);

        var paged = new PagedResponse<FilmItemResponse>(
            items,
            count,
            query.Page,
            query.PageSize
        );

        await Send.OkAsync(paged, ct);
    }
}