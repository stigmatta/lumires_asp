using System.Diagnostics;
using FastEndpoints;
using JetBrains.Annotations;
using ZiggyCreatures.Caching.Fusion;

namespace Api.ToDelete;

public class GetMovieByIdEndpoint(IFusionCache cache) : Endpoint<MovieRequest, MovieResponse>
{
    public override void Configure()
    {
        Get("/cache/movies/{ID}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(MovieRequest req, CancellationToken ct)
    {
        Debug.Assert(req != null, nameof(req) + " != null");
        var movieName = await cache.GetOrSetAsync(
            $"movie-{req.ID}",
            async cancel => await FakeDbCall(req.ID, cancel),
            tags: ["movie"],
            token: ct);

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
public record MovieRequest
{
    public int ID { get; set; }
}

public record MovieResponse
{
    public string Title { get; set; }
}