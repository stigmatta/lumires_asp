using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Movies.GetThisWeekPopular;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetThisWeekPopular";
        Description = """
                      Returns a list of top rated movies this week, ordered by Bayesian average score.

                      Combines TMDB `vote_average` and `vote_count` to calculate a weighted rating,
                      preventing movies with few votes from ranking above well-known titles.

                      ### Headers

                      - **Accept-Language** — Preferred language (e.g., `uk-UA`, `en-US`)
                      """;

        Response(200, "Top rated movies retrieved successfully.", example: new Response(
        [
            new WeeklyPopularItem(
                new Guid("28f619dc-f235-4232-b580-e71b6481109c"),
                550,
                "Fight Club",
                22500,
                "fight-club-1999",
                "some-trailer-url",
                "/5TiwfWEaPSwD20uwXjCTUqpQX70.jpg"
            ),
            new WeeklyPopularItem(
                new Guid("91a3b2cd-e456-7890-c123-f45d6789012e"),
                238,
                "The Godfather",
                3252,
                "the-godfather-1972",
                "some-trailer-url",
                "/rSPw7tgCH9c6NqIHkYPJ8MqK9s4.jpg"
            )
        ]));
        Response(500);
    }
}