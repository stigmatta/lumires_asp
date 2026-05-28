using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using ZiggyCreatures.Caching.Fusion;

namespace lumires.Api.Features.Films.GetThisWeekPopular;

[UsedImplicitly]
internal sealed record WeeklyPopularItem(
    int ExternalId,
    string Title,
    int? ReleaseYear,
    int VoteCount,
    string Slug,
    string? TrailerUrl,
    string? BackdropPath);

[UsedImplicitly]
internal sealed record Response(IReadOnlyList<WeeklyPopularItem> Items);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    IFusionCache cache,
    DataAccess dataAccess)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/films/popular/weekly");
        Description(x => x.WithTags("Films"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var cacheKey = CacheKeys.ThisWeekPopularFilms(lang);

        Response = await cache.GetOrSetAsync<Response>(
            cacheKey,
            async (_, token) =>
            {
                var result = await dataAccess.GetThisWeekPopular(lang, token);
                return result ?? new Response([]);
            },
            options => options.SetDuration(CacheDuration.Long).SetFailSafe(true),
            ct
        );
    }
}