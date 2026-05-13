using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.LikeReview;

[UsedImplicitly]
internal sealed record Query(Guid ReviewId);

[UsedImplicitly]
internal sealed record Response(bool IsLiked, int LikesCount);

internal sealed class Endpoint(DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Post("/films/{slug}/{filmId:int}/reviews/{reviewId}/like");
        Description(x => x.WithTags("Reviews"));
        Throttle(5, 2);
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var response = await db.ToggleLikeAsync(query.ReviewId, ct);
        if (!response.IsSuccess)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(response.Value, ct);
    }
}