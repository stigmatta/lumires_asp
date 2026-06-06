using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.LikeFilm;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "LikeFilm";
        Description = """
                      Likes a film with a current user id.

                      If already liked - remove like.

                      Can return 429 if too more than 4 request were made in a span of 2 seconds from a current user.

                      Returns button state (is liked or not) and likes count

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Query(Guid.CreateVersion7());

        Response(200, "Film is successfully liked");
        Response(404);
        Response(429);
        Response(401);
    }
}