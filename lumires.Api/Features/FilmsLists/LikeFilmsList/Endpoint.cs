using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmsLists.LikeFilmsList;

[UsedImplicitly]
internal sealed record Query(Guid ListId);

[UsedImplicitly]
internal sealed record Response(bool IsLiked, int LikesCount);

internal sealed class Endpoint(DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Post("/lists/{ListId}/like");
        Description(x => x.WithTags("Lists"));
        Throttle(5, 2);
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var response = await db.ToggleLikeAsync(query.ListId, ct);
        if (!response.IsSuccess)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(response.Value, ct);
    }
}