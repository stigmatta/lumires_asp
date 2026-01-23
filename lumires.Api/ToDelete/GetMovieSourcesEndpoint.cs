using FastEndpoints;
using lumires.Api.Core.Abstractions;
using lumires.Api.Core.Models;

namespace lumires.Api.ToDelete;

internal class GetMovieSourcesEndpoint(IStreamingService streamingService)
    : EndpointWithoutRequest<List<MovieSource>>
{
    public override void Configure()
    {
        Get("/api/movies/{tmdbId}/sources");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var tmdbId = Route<string>("tmdbId");

        if (string.IsNullOrEmpty(tmdbId))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var sources = await streamingService.GetSourcesAsync(tmdbId);

        if (sources.Count == 0)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(sources, ct);
    }
}