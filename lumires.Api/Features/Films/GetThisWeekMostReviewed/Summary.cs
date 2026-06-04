using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.GetThisWeekMostReviewed;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetThisWeekMostReviewed";
        Description = """
                      Returns preview list of six the most reviewed films this week.
                      ### Headers

                      - **Accept-Language** — Preferred language (e.g., `uk-UA`, `en-US`)
                      """;

        Response(200, "Most reviewed films this week retrieved successfully.", example: new Response(
        [
            new WeeklyReviewedItem(
                550,
                Guid.CreateVersion7(),
                "Fight Club",
                "Loved this so much",
                "fight-club-1999",
                "some-backdrop-path",
                Guid.Empty,
                "user-poser",
                5
            ),
            new WeeklyReviewedItem(
                550,
                Guid.CreateVersion7(),
                "The Godfather",
                "Loved this even more",
                "the-godfather-1972",
                "some-backdrop-path",
                Guid.Empty,
                "user-poser",
                5
            )
        ]));
        Response(500);
    }
}