using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.UnrateFilm;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "UnrateMovie";
        Description = """
                      Removes a rating for a specific movie from a current user.

                      If the film was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(
            550
        );
        Response(204, "Film is successfully rated");
        Response(400);
        Response(404);
    }
}