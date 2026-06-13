using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.WatchFilm;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "WatchFilm";
        Description = """
                      Toggles the watched state of a specific movie for the current user.

                      If the film was not marked as watched, it is marked as watched and the response is `{ "isWatched": true }`.

                      If the film was already marked as watched, the mark is removed and the response is `{ "isWatched": false }`.

                      If the film was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(550);
        Response<Response>(200, "Watched state toggled successfully.");
        Response(400);
        Response(404);
    }
}