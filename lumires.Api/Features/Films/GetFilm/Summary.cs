using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.GetFilm;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetFilm";
        Description = """
                      Returns film model with localized details.

                      If the film isn't in the local database, it fetches it from TMDB and triggers an import with all its localized versions.

                      ### Route parameters

                      - **Id** — TMDB film identifier

                      ### Headers

                      - **Accept-Language** — Preferred language (e.g., `uk-UA`, `en-US`)
                      - **Authorization Bearer** — Optional. When supplied, `isLikedByMe` and `isWatchedByMe` reflect the current user's state; otherwise both are `false`.
                      """;

        ExampleRequest = new Query(550);
        Response(200, "Film details retrieved successfully.");
        Response(404);
        Response(500);
    }
}