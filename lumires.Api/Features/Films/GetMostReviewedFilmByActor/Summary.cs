using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.GetMostReviewedFilmByActor;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetMostReviewedFilmByActor";
        Description = """
                      Returns a film that`s been the most discussed from all time from the specific actor.

                      Can return 204 if no review

                      ### Route parameters

                      - **Id** — Actor identifier

                      ### Headers

                      - **Accept-Language** — Preferred language (e.g., `uk-UA`, `en-US`)
                      """;

        ExampleRequest = new Query(550);
        Response(200, "Film retrieved successfully.");
        Response(401);
        Response(404);
        Response(500);
    }
}