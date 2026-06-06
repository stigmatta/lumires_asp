using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.LikeFilm;

[UsedImplicitly]
internal sealed record Query(Guid Id);

[UsedImplicitly]
internal sealed record Response(bool IsLiked, int LikesCount);

internal sealed class Endpoint(DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Post("/films/{Slug}/{Id:int}/like");
        Description(x => x.WithTags("Films"));
        Throttle(5, 2);
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var response = await db.ToggleLikeAsync(query.Id, ct);
        if (!response.IsSuccess)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(response.Value, ct);
    }
}