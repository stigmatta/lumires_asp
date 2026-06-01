using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Constants;
using ZiggyCreatures.Caching.Fusion;

namespace lumires.Api.Features.FilmsLists.GetFilmsListsSummary;

[UsedImplicitly]
internal sealed record Response(long ListsTotal, long ListsThisDay);

internal sealed class Endpoint(
    DataAccess db,
    IFusionCache cache)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/lists/summary");
        Description(x => x.WithTags("Lists"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var cacheKey = CacheKeys.ReviewsSummary();
        const int day = 1;


        Response = await cache.GetOrSetAsync<Response>(
            cacheKey,
            async (_, token) =>
            {
                var countToday = await db.GetListsFromSpan(day, token);
                var countTotal = await db.GetListsTotalCount(token);

                return new Response(countTotal, countToday);
            },
            options => options.SetDuration(CacheDuration.Medium)
                .SetFailSafe(true),
            ct
        );
    }
}