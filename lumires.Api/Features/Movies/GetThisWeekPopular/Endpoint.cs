using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using ZiggyCreatures.Caching.Fusion;

namespace lumires.Api.Features.Movies.GetThisWeekPopular;

[UsedImplicitly]
internal sealed record WeeklyPopularItem(
    Guid Id,
    int ExternalId,
    string Title,
    int VoteCount,
    string Slug,
    string? TrailerUrl,
    string? BackdropPath);

[UsedImplicitly]
internal sealed record Response(IReadOnlyList<WeeklyPopularItem> Items);

[UsedImplicitly]
internal sealed record LocalizationResponse(
    string LanguageCode,
    string Title
);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    IFusionCache cache,
    DataAccess dataAccess)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/movies/popular-this-week");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var cacheKey = CacheKeys.ThisWeekPopularMovies(lang);

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