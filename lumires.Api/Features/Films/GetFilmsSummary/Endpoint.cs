using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Domain.Exceptions;
using ZiggyCreatures.Caching.Fusion;

namespace lumires.Api.Features.Films.GetFilmsSummary;

[UsedImplicitly]
internal sealed record Response(long FilmCount, int GenreCount);

internal sealed class Endpoint(
    IExternalFilmService externalFilmService,
    DataAccess db,
    IFusionCache cache)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/films/summary");
        Description(x => x.WithTags("Films"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var cacheKey = CacheKeys.FilmsSummary();
        try
        {
            Response = await cache.GetOrSetAsync<Response>(
                cacheKey,
                async (_, token) =>
                {
                    var totalFilmsCount = await externalFilmService.GetTotalFilmsCountAsync(token);
                    var totalGenresCount = await db.GetTotalGenresCount(token);

                    return new Response(totalFilmsCount, totalGenresCount);
                },
                options => options.SetDuration(CacheDuration.Eternal)
                    .SetFailSafe(true),
                ct
            );
        }
        catch (ResultException ex)
        {
            await HttpContext.SendErrorAsync(ex.Status, ct);
        }
    }
}