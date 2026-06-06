using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Reviews.GetThisWeekTrendingReviews;

[UsedImplicitly]
internal sealed record Response(IReadOnlyList<TrendingReviewItem> Items);

[UsedImplicitly]
internal sealed record TrendingReviewItem(
    Guid Id,
    int FilmId,
    string FilmTitle,
    string FilmSlug,
    string ReviewTitle,
    float? Rating,
    Guid UserId,
    string Username
);

internal sealed class Endpoint(ICurrentUserService currentUserService, DataAccess db)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/reviews/trending/weekly");
        Description(x => x.WithTags("Reviews"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;

        var response = await db.GetTrendingReviewsWeeklyAsync(lang, ct);

        if (response is null)
        {
            await Send.OkAsync(new Response([]), ct);
            return;
        }

        await Send.OkAsync(response, ct);
    }
}