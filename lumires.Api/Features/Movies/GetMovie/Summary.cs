using FastEndpoints;
using JetBrains.Annotations;

namespace Api.Features.Movies.GetMovie;

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
        Response(200, "Movie details retrieved successfully.", example: new Response(
            550,
            1999,
            "dfeUzm6KF4g",
            "/pB8BM7pdSp6B6Ih7QZ4DrQ3PmJK.jpg",
            "/5TiwfWEaPSwD20uwXjCTUqpQX70.jpg",
            new LocalizationResponse("en", "Fight Club", "An insomniac office worker...")
        ));
        Response(404);
        Response(500);
    }
}