using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.GetThisWeekRecentReleases;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetThisWeekRecentReleases";
        Description = """
                      Returns a list of recently released movies from the last 30 days, ordered by release date descending.

                      Titles are returned in the preferred language when available, falling back to the default language.

                      ### Headers

                      - **Accept-Language** — Preferred language (e.g., `uk-UA`, `en-US`)
                      """;

        Response(200, "Recently released movies retrieved successfully.", example: new Response(
        [
            new WeeklyRecentItem(
                1022789,
                "Inside Out 2",
                8420,
                "inside-out-some-year",
                "some-url",
                "/xg27NrXi7VXCGUr7MG75UqLl5Vg.jpg"
            ),
            new WeeklyRecentItem(
                748783,
                "The Garfield Movie",
                3105,
                "the-garfield-movie-some-year",
                "some-url",
                "/fgsHxzACcBRpDLZJ9gTSMPSHLad.jpg"
            )
        ]));
        Response(500);
    }
}