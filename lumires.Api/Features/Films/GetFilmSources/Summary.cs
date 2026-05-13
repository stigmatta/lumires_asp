using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Models;

namespace lumires.Api.Features.Films.GetFilmSources;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetFilmSources";
        Description = """
                      Returns a list of available streaming sources (platforms, torrents, etc.) for the specified movie.

                      The endpoint checks the cache first (FusionCache) and fetches data from the external Watchmode API if needed.

                      ### Route parameters

                      - **Id** — TMDB movie identifier

                      """;

        ExampleRequest = new Query(550);
        Response(200, "Movie sources retrieved successfully.", example: new Response([
            new FilmSource(
                550,
                "Netflix",
                "Subscription",
                new Uri("https://www.netflix.com/watch/550"),
                "HD",
                0
            ),

            new FilmSource(
                550,
                "Amazon Prime",
                "Rental",
                new Uri("https://www.amazon.com/dp/B08XYZ"),
                "HD",
                3.99
            )
        ]));
        Response(404, "No sources found for the specified movie.");
        Response(500, "External service error or internal server error.");
    }
}