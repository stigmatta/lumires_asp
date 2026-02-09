using System.Diagnostics;
using FastEndpoints;
using JetBrains.Annotations;
using ZiggyCreatures.Caching.Fusion;

namespace lumires.Api.ToDelete;

[UsedImplicitly]
public record InvalidateMovieCacheRequest
{
    public int MovieId { get; set; }
}

public class InvalidateMovieCacheEndpoint(IFusionCache cache) : Endpoint<InvalidateMovieCacheRequest>
{
    public override void Configure()
    {
        Delete("/cache/movies/{MovieId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(InvalidateMovieCacheRequest req, CancellationToken ct)
    {
        Debug.Assert(req != null, nameof(req) + " != null");

        var tag = $"movie-{req.MovieId}";

        await cache.RemoveAsync(tag, token: ct);

        await Send.NoContentAsync(ct);
    }
}