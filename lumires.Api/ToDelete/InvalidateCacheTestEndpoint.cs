using System.Diagnostics;
using JetBrains.Annotations;

namespace lumires.Api.ToDelete;

using FastEndpoints;
using Microsoft.Extensions.Caching.Hybrid;

[UsedImplicitly]
public record InvalidateMovieCacheRequest
{
    public int MovieId { get; set; }
}

public class InvalidateMovieCacheEndpoint(HybridCache cache) : Endpoint<InvalidateMovieCacheRequest>
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

        await cache.RemoveByTagAsync(tag, ct);

        await Send.NoContentAsync(ct);
    }
}