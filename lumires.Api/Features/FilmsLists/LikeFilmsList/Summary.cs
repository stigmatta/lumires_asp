using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmsLists.LikeFilmsList;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "LikeFilmsList";
        Description = """
                      Likes a list with a current user id.

                      If already liked - remove like.

                      Can return 429 if too more than 4 request were made in a span of 2 seconds from a current user.

                      Returns button state (is liked or not) and likes count

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Query(Guid.CreateVersion7());

        Response(200, "List is successfully liked");
        Response(404);
        Response(429);
        Response(401);
    }
}