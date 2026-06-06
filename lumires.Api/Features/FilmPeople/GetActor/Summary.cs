using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmPeople.GetActor;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetActor";
        Description = """
                      Returns actor`s details with localization fallbacks.

                      If the actor's data is missing from the local database, it fetches the profile from TMDB and ensures the person record is created.

                      ### Route parameters

                      - **Slug** — URL-friendly name of the actor
                      - **Id** — TMDB person identifier

                      ### Headers

                      - **Accept-Language** — Preferred language culture (e.g., `uk-UA`, `en-US`)
                      """;

        ExampleRequest = new Query(1);
        Response<Response>(200, "Actor details retrieved successfully.");
        Response(404, "Actor not found.");
        Response(500, "Internal server error.");
    }
}