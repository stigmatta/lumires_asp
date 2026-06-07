using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetTrendingReviewsByFilm;

[UsedImplicitly]
internal sealed record Query(int FilmId);

[UsedImplicitly]
internal sealed record Response(IReadOnlyList<TrendingReviewItem> Items);

[UsedImplicitly]
internal sealed record TrendingReviewItem(
    Guid Id,
    string? ReviewTitle,
    Guid UserId,
    string Username,
    int LikesCount
);

internal sealed class Endpoint(DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/films/{filmId:int}/reviews/trending");
        Description(x => x.WithTags("Reviews"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var response = await db.GetTrendingReviewsByFilmAsync(query.FilmId, ct);

        if (response is null)
        {
            await Send.OkAsync(new Response([]), ct);
            return;
        }

        await Send.OkAsync(response, ct);
    }
}