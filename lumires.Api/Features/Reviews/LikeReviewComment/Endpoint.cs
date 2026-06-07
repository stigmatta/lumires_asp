using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.LikeReviewComment;

[UsedImplicitly]
internal sealed record Query(Guid ReplyId);

[UsedImplicitly]
internal sealed record Response(bool IsLiked, int LikesCount);

internal sealed class Endpoint(DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Post("/films/{filmId:int}/reviews/{reviewId}/replies/{replyId}/like");
        Description(x => x.WithTags("Reviews"));
        Throttle(5, 2);
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var response = await db.ToggleLikeAsync(query.ReplyId, ct);
        if (!response.IsSuccess)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(response.Value, ct);
    }
}