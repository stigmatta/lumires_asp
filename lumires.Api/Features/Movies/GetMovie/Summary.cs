using FastEndpoints;
using JetBrains.Annotations;

namespace Api.Features.Movies.GetMovie;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "Retrieve movie details by TMDB ID";
        Description = """
                      Returns movie model with localized details.

                      If the movie isn't in the local database, it fetches it from TMDB and triggers an import with all its localized versions.

                      ### Headers

                      - **Accept-Language** — Preferred language (e.g., `uk-UA`, `en-US`)
                      """;

        ExampleRequest = new Request(550); 
        Response(200, "Movie details retrieved successfully.", example: new Response(
            Id: 550,
            Year: 1999,
            Localization: new LocalizationResponse("en", "Fight Club", "An insomniac office worker...")
        ));
        Response(404);
        Response(500);
    }
}