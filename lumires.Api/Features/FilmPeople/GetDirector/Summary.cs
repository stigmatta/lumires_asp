namespace lumires.Api.Features.FilmPeople.GetDirector;

using FastEndpoints;
using JetBrains.Annotations;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetDirector";
        Description = """
                      Returns director details with localization fallbacks.

                      If the director's data is missing from the local database, it fetches the profile from TMDB and ensures the person record is created.

                      ### Route parameters

                      - **Slug** — URL-friendly name of the director
                      - **Id** — TMDB person identifier

                      ### Headers

                      - **Accept-Language** — Preferred language culture (e.g., `uk-UA`, `en-US`)
                      """;

        ExampleRequest = new Query(1);
        Response<Response>(200, "Director details retrieved successfully.");
        Response(404, "Director not found.");
        Response(500, "Internal server error.");
    }
}