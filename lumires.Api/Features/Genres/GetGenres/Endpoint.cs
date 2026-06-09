using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Features.Genres.Contracts;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using ZiggyCreatures.Caching.Fusion;

namespace lumires.Api.Features.Genres.GetGenres;

[UsedImplicitly]
internal sealed record Response(
    IReadOnlyCollection<GenreItem> Genres
);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    IFusionCache cache,
    DataAccess dataAccess)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/genres/");
        Description(x => x.WithTags("Genres"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var cacheKey = CacheKeys.GenresList(lang);

        Response = await cache.GetOrSetAsync<Response>(
            cacheKey,
            async (_, token) =>
            {
                var genres = await dataAccess.GetGenres(lang, token);

                return genres;
            },
            options => options.SetDuration(CacheDuration.Eternal).SetFailSafe(true),
            ct);
    }
}