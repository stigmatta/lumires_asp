using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.AddFilmToWatchlist;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "AddFilmToWatchlist";
        Description = """
                      Adds a specific film to the current user's watchlist only once.

                      If the film was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.

                      If already in the watchlist - 204 No Content

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(550);
        Response(204, "Film is successfully added to the watchlist");
        Response(400);
        Response(404);
    }
}
