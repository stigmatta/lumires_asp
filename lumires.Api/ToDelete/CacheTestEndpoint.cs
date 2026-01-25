using System.Diagnostics;
using JetBrains.Annotations;

namespace lumires.Api.ToDelete;

using FastEndpoints;
using Microsoft.Extensions.Caching.Hybrid;

public class GetMovieByIdEndpoint(HybridCache cache) : Endpoint<MovieRequest, MovieResponse>
{
    public override void Configure()
    {
        Get("/cache/movies/{ID}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(MovieRequest req, CancellationToken ct)
    {
        Debug.Assert(req != null, nameof(req) + " != null");
        var movieName = await cache.GetOrCreateAsync(
            key: $"movie-{req.ID}",
            factory: async cancel => await FakeDbCall(req.ID, cancel),
            cancellationToken: ct,
            tags:["movie"]
        );

        await Send.OkAsync(new MovieResponse { Title = movieName }, ct);
    }

    private async Task<string> FakeDbCall(int id, CancellationToken ct)
    {
        if (id == 999) ThrowError("Внешний сервис временно недоступен", 502); 
        
        await Task.Delay(50, ct); 
        return $"Movie {id}";
    }
}

[UsedImplicitly]
public record MovieRequest { public int ID { get; set; } }
public record MovieResponse { public string Title { get; set; } }