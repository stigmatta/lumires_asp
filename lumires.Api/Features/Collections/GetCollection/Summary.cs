using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Collections.GetCollection;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetCollection";
        Description = """
                      Returns a collection with its movies and localized details.

                      Available to anonymous users. Private collections are only visible to their owner.

                      ### Route parameters

                      - **Id** — Collection identifier

                      ### Headers

                      - **Accept-Language** — Preferred language (e.g., `uk-UA`, `en-US`)
                      """;

        ExampleRequest = new Query(Guid.NewGuid());

        Response(200, "Collection retrieved successfully.", example: new Response(
            Guid.NewGuid(),
            "My Favourite Movies",
            "A list of movies I love.",
            "morrigun01",
            DateTimeOffset.UtcNow,
            [
                new CollectionMovieItem(550, "Fight Club", "/pB8BM7pdSp6B6Ih7QZ4DrQ3PmJK.jpg", 1),
                new CollectionMovieItem(551, "Inception", "/9gk7adHYeDvHkCSEqAvQNLV5Uge.jpg", 2)
            ]
        ));
        Response(404);
        Response(500);
    }
}