using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmPeople.GetDirectorStats;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetDirectorStats";
        Description = """
                      Returns aggregate stats for a director sourced from TMDB:

                      - **FilmsCount** — number of films credited as director
                      - **AverageRating** — mean rating (0–5 scale) across the director's
                        films that have at least one vote, rounded to 1 decimal
                      - **Awards** — total **Nominations** and **Wins**

                      ### Awards source

                      TMDB exposes no awards API, so award counts are scraped from the public
                      TMDB website awards page. This is best-effort: if the page is unavailable,
                      `Awards` is returned as `null` while film stats are still returned.

                      ### Route parameters

                      - **Id** — TMDB person identifier

                      ### Headers

                      - **Accept-Language** — Preferred language culture (e.g., `uk-UA`, `en-US`)
                      """;

        ExampleRequest = new Query(108);
        Response<Response>(200, "Director stats retrieved successfully.");
        Response(404, "Director not found.");
        Response(500, "Internal server error.");
    }
}
