using Core.Abstractions.Services;
using Core.Models;
using FastEndpoints;
using JetBrains.Annotations;

namespace Api.Features.Movies.GetMovieSources;

[UsedImplicitly]
internal sealed record Query(int Id);

[UsedImplicitly]
internal sealed record Response(List<MovieSource> Sources);

internal sealed class Endpoint(IStreamingService streamingService)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/movies/{Id:int}/sources");
        AllowAnonymous(); //TODO maybe make this endpoint subscription only to save limits ?
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var sources = await streamingService.GetSourcesAsync(query.Id, ct);

        if (!sources.IsSuccess)
        {
            await HttpContext.SendErrorAsync(sources, ct);
            return;
        }

        if (sources.Value.Count == 0)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        Response = new Response(sources);
    }
}