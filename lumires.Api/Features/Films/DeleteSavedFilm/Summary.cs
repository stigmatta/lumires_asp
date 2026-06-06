using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.DeleteSavedFilm;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "DeleteSavedFilm";
        Description = """
                      Deletes a specific film as a saved.

                      If the film was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(550);
        Response(204, "Film is successfully deleted as a saved");
        Response(400);
        Response(404);
    }
}