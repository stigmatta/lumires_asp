using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmsLists.DeleteFilmsList;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "DeleteFilmsList";
        Description = """
                      Deletes a specific list.

                      If the list was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.
                      
                      If its not yours - 403 Forbidden

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(Guid.CreateVersion7());
        Response(204, "List is successfully deleted");
        Response(400);
        Response(403);
        Response(404);
    }
}