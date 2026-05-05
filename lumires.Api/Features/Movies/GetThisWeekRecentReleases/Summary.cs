using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Movies.GetThisWeekRecentReleases;

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
                new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                1022789,
                "Inside Out 2",
                8420,
                "/xg27NrXi7VXCGUr7MG75UqLl5Vg.jpg"
            ),
            new WeeklyRecentItem(
                new Guid("7cb12d34-a891-4567-d234-e56f7890123a"),
                748783,
                "The Garfield Movie",
                3105,
                "/fgsHxzACcBRpDLZJ9gTSMPSHLad.jpg"
            )
        ]));
        Response(500);
    }
}