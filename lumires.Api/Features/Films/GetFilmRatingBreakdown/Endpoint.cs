using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.GetFilmRatingBreakdown;

[UsedImplicitly]
internal sealed record Query(int Id);
internal sealed class Endpoint(DataAccess db)
    : Endpoint<Query, Dictionary<float, int>>
{
    public override void Configure()
    {
        Get("/films/rating-breakdown");
        Description(x => x.WithTags("Films"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var items = await db.GetFilmRatingsDictionaryAsync(query.Id, ct);
        await Send.OkAsync(items, ct);
    }

}