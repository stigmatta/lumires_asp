using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.DeleteFromWatchlist;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "DeleteFromWatchlist";
        Description = """
                      Removes a specific film from the current user's watchlist.

                      If the film was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(550);
        Response(204, "Film is successfully removed from the watchlist");
        Response(400);
        Response(404);
    }
}
