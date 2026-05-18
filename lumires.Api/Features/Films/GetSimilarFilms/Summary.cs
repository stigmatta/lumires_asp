using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.GetSimilarFilms;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetSimilarFilms";
        Description = """
                      Returns a list of films similar to the specified film.

                      Fetches similar films from TMDB and asynchronously enqueues any that are not yet in the local database for import.

                      ### Route parameters

                      - **Id** — TMDB film identifier

                      ### Headers

                      - **Accept-Language** — Preferred language (e.g., `uk-UA`, `en-US`)
                      """;

        ExampleRequest = new Query(550);
        Response(200, "Similar films retrieved successfully.");
        Response(401);
        Response(404);
        Response(500);
    }
}