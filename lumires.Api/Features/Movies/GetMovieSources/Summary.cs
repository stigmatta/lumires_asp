using Core.Models;
using FastEndpoints;
using JetBrains.Annotations;

namespace Api.Features.Movies.GetMovieSources;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetMovieSources";
        Description = """
                      Returns a list of available streaming sources (platforms, torrents, etc.) for the specified movie.

                      The endpoint checks the cache first (FusionCache) and fetches data from the external Watchmode API if needed.

                      ### Route parameters

                      - **Id** — TMDB movie identifier

                      """;

        ExampleRequest = new Query(550);
        Response(200, "Movie sources retrieved successfully.", example: new Response([
            new MovieSource(
                ExternalId: 550,
                ProviderName: "Netflix",
                Type: "Subscription",
                Url: new Uri("https://www.netflix.com/watch/550"),
                Quality: "HD",
                Price: 0
            ),

            new MovieSource(
                ExternalId: 550,
                ProviderName: "Amazon Prime",
                Type: "Rental",
                Url: new Uri("https://www.amazon.com/dp/B08XYZ"),
                Quality: "HD",
                Price: 3.99
            )
        ]));
        Response(404, "No sources found for the specified movie.");
        Response(500, "External service error or internal server error.");
    }
}