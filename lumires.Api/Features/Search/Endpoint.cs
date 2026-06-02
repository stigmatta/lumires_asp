using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Core.Models;

namespace lumires.Api.Features.Search;

[UsedImplicitly]
internal enum ContentType
{
    All,
    Films,
    Directors,
    Actors,
    Lists,
    Members
}

[UsedImplicitly]
internal sealed class Query
{
    public ContentType? Filter { get; init; } = ContentType.All;
    public string SearchTerm { get; init; } = string.Empty;
    public int Page { get; init; } = 1;
}

[UsedImplicitly]
internal sealed record FilmInListItem(
    string? PosterPath);

[UsedImplicitly]
internal sealed record ListResult(
    Guid Id,
    string Title,
    Guid UserId,
    string Username,
    int FilmCount,
    int LikeCount,
    IReadOnlyCollection<FilmInListItem> Films);

[UsedImplicitly]
internal sealed record MemberResult(
    Guid Id,
    string Username,
    string? AvatarUrl,
    int FollowersCount
);

[UsedImplicitly]
internal sealed record SearchResponse(
    IReadOnlyList<ExternalFilmShort>? Films = null,
    IReadOnlyList<ExternalPersonShort>? Directors = null,
    IReadOnlyList<ExternalPersonShort>? Actors = null,
    IReadOnlyList<ListResult>? Lists = null,
    IReadOnlyList<MemberResult>? Members = null
);

[UsedImplicitly]
internal sealed class Endpoint(DataAccess db, ISearchService searchService, ICurrentUserService currentUserService)
    : Endpoint<Query, SearchResponse>
{
    public override void Configure()
    {
        Get("/search");
        Description(x => x.WithTags("Search"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;

        if (string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            await Send.OkAsync(ct);
            return;
        }

        var search = query.SearchTerm.ToLowerInvariant();

        var response = query.Filter switch
        {
            ContentType.Films => await GetFilmsSearchAsync(lang, search, query.Page, ct),
            ContentType.Directors => await GetDirectorsSearchAsync(lang, search, query.Page, ct),
            ContentType.Actors => await GetActorsSearchAsync(lang, search, query.Page, ct),
            ContentType.Lists => await db.GetLists(search, ct),
            ContentType.Members => await db.GetMembers(search, ct),
            _ => await GetCombinedSearchAsync(lang, search, query.Page, ct)
        };

        await Send.OkAsync(response, ct);
    }


    private async Task<SearchResponse> GetFilmsSearchAsync(string lang, string searchTerm, int page,
        CancellationToken ct)
    {
        var response = await searchService.SearchFilmsAsync(lang, searchTerm, page, ct);
        return new SearchResponse(response.Value);
    }

    private async Task<SearchResponse> GetDirectorsSearchAsync(string lang, string searchTerm, int page,
        CancellationToken ct)
    {
        var response = await searchService.SearchDirectorsAsync(lang, searchTerm, page, ct);
        return new SearchResponse(Directors: response.Value);
    }

    private async Task<SearchResponse> GetActorsSearchAsync(string lang, string searchTerm, int page,
        CancellationToken ct)
    {
        var response = await searchService.SearchActorsAsync(lang, searchTerm, page, ct);
        return new SearchResponse(Actors: response.Value);
    }

    private async Task<SearchResponse> GetCombinedSearchAsync(
        string lang,
        string search,
        int page,
        CancellationToken ct)
    {
        var tmdbTask = searchService.SearchAllAsync(lang, search, page, ct);
        var localTask = db.GetAll(search, ct);

        await Task.WhenAll(tmdbTask, localTask);

        var tmdb = await tmdbTask;
        var local = await localTask;

        return new SearchResponse(
            tmdb.Value.Films,
            Actors: tmdb.Value.Actors,
            Directors: tmdb.Value.Directors,
            Lists: local.Lists,
            Members: local.Members
        );
    }
}