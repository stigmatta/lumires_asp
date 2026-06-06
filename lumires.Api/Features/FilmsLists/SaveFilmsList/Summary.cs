using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmsLists.SaveFilmsList;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "SaveFilmsList";
        Description = """
                      Saves a specific list.

                      If the list was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.

                      If already saved - 204 No Content

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(Guid.CreateVersion7());
        Response(204, "Film is successfully marked as saved");
        Response(400);
        Response(404);
    }
}