using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Constants;
using ZiggyCreatures.Caching.Fusion;

namespace lumires.Api.Features.Threads.GetThreadsSummary;

[UsedImplicitly]
internal sealed record Response(long ThreadsThisWeek, long ThreadsThisDay);

internal sealed class Endpoint(
    DataAccess db,
    IFusionCache cache)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/threads/summary");
        Description(x => x.WithTags("Threads"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var cacheKey = CacheKeys.ThreadsSummary();
        const int week = 7;
        const int day = 1;


        Response = await cache.GetOrSetAsync<Response>(
            cacheKey,
            async (_, token) =>
            {
                var countThisWeek = await db.GetThreadsFromDaySpan(week, token);
                var countToday = await db.GetThreadsFromDaySpan(day, token);

                return new Response(countThisWeek, countToday);
            },
            options => options.SetDuration(CacheDuration.Eternal)
                .SetFailSafe(true),
            ct
        );
    }
}