using Contracts.Abstractions;
using Contracts.Models;
using FastEndpoints;

namespace lumires.Api.ToDelete;

internal class GetMovieSourcesEndpoint(IStreamingService streamingService)
    : EndpointWithoutRequest<List<MovieSource>>
{
    public override void Configure()
    {
        Get("/api/movies/{tmdbId:int}/sources");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var tmdbId = Route<int>("tmdbId");

        var sources = await streamingService.GetSourcesAsync(tmdbId, ct);

        if (sources.Count == 0)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(sources, ct);
    }
}