using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmsLists.GetFilmsListsByFilmPreview;

[UsedImplicitly]
internal sealed record Query(int Id);

[UsedImplicitly]
internal sealed record Response(IReadOnlyCollection<FilmsListsItems> FilmLists);

[UsedImplicitly]
internal sealed record FilmsListsItems(IReadOnlyCollection<FilmListItem> Films, string Name);

[UsedImplicitly]
internal sealed record FilmListItem(string? BackdropPath);

internal sealed class Endpoint(
    DataAccess db) : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/films/{Slug}/{Id:int}/lists");
        Description(x => x.WithTags("Lists"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var filmId = query.Id;

        var response = await db.GetFilmListsByFilmIdAsync(filmId, ct);

        await Send.OkAsync(response, ct);
    }
}