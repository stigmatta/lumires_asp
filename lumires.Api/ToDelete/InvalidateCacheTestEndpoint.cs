using System.Diagnostics;
using Core.Abstractions.Services;
using Core.Constants;
using FastEndpoints;
using JetBrains.Annotations;
using ZiggyCreatures.Caching.Fusion;

namespace Api.ToDelete;

[UsedImplicitly]
public record InvalidateMovieCacheRequest
{
    public int MovieId { get; set; }
}

public class InvalidateMovieCacheEndpoint(IFusionCache cache, ICurrentUserService userService)
    : Endpoint<InvalidateMovieCacheRequest>
{
    public override void Configure()
    {
        Delete("/cache/movies/{MovieId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(InvalidateMovieCacheRequest req, CancellationToken ct)
    {
        Debug.Assert(req != null, nameof(req) + " != null");

        var key = CacheKeys.MovieKey(req.MovieId, userService.LangCulture);

        await cache.RemoveAsync(key, token: ct);

        await Send.NoContentAsync(ct);
    }
}