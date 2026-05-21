using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using ZiggyCreatures.Caching.Fusion;

namespace lumires.Api.Features.Films.GetThisWeekRecentReleases;

[UsedImplicitly]
internal sealed record WeeklyRecentItem(
    int ExternalId,
    string Title,
    int VoteCount,
    int ReleaseYear,
    string Slug,
    string? TrailerUrl,
    string? BackdropPath);

[UsedImplicitly]
internal sealed record Response(IReadOnlyList<WeeklyRecentItem> Items);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    IFusionCache cache,
    DataAccess dataAccess)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/films/recent/weekly");
        Description(x => x.WithTags("Films"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var cacheKey = CacheKeys.ThisWeekRecentFilms(lang);

        Response = await cache.GetOrSetAsync<Response>(
            cacheKey,
            async (_, token) =>
            {
                var result = await dataAccess.GetThisWeekRecentReleases(lang, token);
                return result ?? new Response([]);
            },
            options => options.SetDuration(CacheDuration.Long).SetFailSafe(true),
            ct
        );
    }
}