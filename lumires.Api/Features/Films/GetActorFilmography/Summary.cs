using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.GetActorFilmography;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetActorFilmography";
        Description = """
                      Returns 
                      Returns the actor`s filmography with the short info about the films.

                      ### Route parameters

                      - **Id** — TMDB actor id

                      ### Headers

                      - **Accept-Language** — Preferred language (e.g., `uk-UA`, `en-US`)
                      """;

        ExampleRequest = new Query(550);
        Response(200, "Filmography retrieved successfully.");
        Response(404);
        Response(500);
    }
}