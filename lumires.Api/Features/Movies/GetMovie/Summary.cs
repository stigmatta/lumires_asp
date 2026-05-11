using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Movies.GetMovie;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetMovie";
        Description = """
                      Returns movie model with localized details.

                      If the movie isn't in the local database, it fetches it from TMDB and triggers an import with all its localized versions.

                      ### Route parameters

                      - **Id** — TMDB movie identifier

                      ### Headers

                      - **Accept-Language** — Preferred language (e.g., `uk-UA`, `en-US`)
                      """;

        ExampleRequest = new Query(550);
        Response(200, "Movie details retrieved successfully.");
        Response(404);
        Response(500);
    }
}