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
                      - **Authorization Bearer** — Optional. When supplied, `isLikedByMe`, `isWatchedByMe` and `myRating` reflect the current user's state; otherwise `isLikedByMe`/`isWatchedByMe` are `false` and `myRating` is `null`. `myRating` is also `null` when the authenticated user hasn't rated the film.
                      """;

        ExampleRequest = new Query(550);
        Response(200, "Film details retrieved successfully.");
        Response(404);
        Response(500);
    }
}