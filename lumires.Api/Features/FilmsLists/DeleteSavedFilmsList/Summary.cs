using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmsLists.DeleteSavedFilmsList;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "DeleteSavedList";
        Description = """
                      Deletes a specific list as a saved.

                      If the list was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(Guid.CreateVersion7());
        Response(204, "List is successfully deleted from saved");
        Response(400);
        Response(404);
    }
}