using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.GetFilmTags;

[UsedImplicitly]
internal sealed record Query(int FilmId);

[UsedImplicitly]
internal sealed record TagItem(Guid Id, string Name, string Slug);

[UsedImplicitly]
internal sealed record Response(IReadOnlyCollection<TagItem> Tags);

internal sealed class Endpoint(DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/films/{Slug}/{Id:int}/tags");
        Description(x => x.WithTags("Films"));
        Throttle(5, 2);
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var result = await db.GetFilmTags(query.FilmId, ct);
        await Send.OkAsync(result, ct);
    }
}