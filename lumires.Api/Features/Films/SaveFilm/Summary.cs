using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.SaveFilm;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "SaveFilm";
        Description = """
                      Marks a specific film as a saved only once.

                      If the film was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.

                      If already watched - 204 No Content

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(550);
        Response(204, "Film is successfully marked as saved");
        Response(400);
        Response(404);
    }
}