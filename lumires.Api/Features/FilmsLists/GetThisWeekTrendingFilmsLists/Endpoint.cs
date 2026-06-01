using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmsLists.GetThisWeekTrendingFilmsLists;

[UsedImplicitly]
internal sealed record Response(IReadOnlyList<TrendingListItem> Items);

[UsedImplicitly]
internal sealed record TrendingListItem(
    Guid Id,
    string Title,
    Guid UserId,
    string Username,
    int FilmCount,
    IReadOnlyCollection<FilmListItem> Films
);

[UsedImplicitly]
internal sealed record FilmListItem(string? PosterPath);

internal sealed class Endpoint(DataAccess db)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/lists/trending/weekly");
        Description(x => x.WithTags("Lists"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = await db.GetTrendingFilmListsWeekly(ct);
        await Send.OkAsync(response, ct);
    }
}