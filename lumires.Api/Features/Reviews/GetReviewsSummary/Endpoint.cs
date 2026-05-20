using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Constants;
using ZiggyCreatures.Caching.Fusion;

namespace lumires.Api.Features.Reviews.GetReviewsSummary;

[UsedImplicitly]
internal sealed record Response(long ReviewsThisWeek, long ReviewsThisDay);

internal sealed class Endpoint(
    DataAccess db,
    IFusionCache cache)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/reviews/summary");
        Description(x => x.WithTags("Reviews"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var cacheKey = CacheKeys.ReviewsSummary();
        const int week = 7;
        const int day = 1;


        Response = await cache.GetOrSetAsync<Response>(
            cacheKey,
            async (_, token) =>
            {
                var countThisWeek = await db.GetReviewsFromDaySpan(week, token);
                var countToday = await db.GetReviewsFromDaySpan(day, token);

                return new Response(countThisWeek, countToday);
            },
            options => options.SetDuration(CacheDuration.Eternal)
                .SetFailSafe(true),
            ct
        );
    }
}