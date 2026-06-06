using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.GetFilmStats;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetFilmStats";
        Description = """
                      Returns stats for a specified film.

                      ### Route parameters

                      - **Id** — TMDB film identifier

                      ### Headers

                      """;

        ExampleRequest = new Query(550);
        Response(200, "Similar films retrieved successfully.");
        Response(401);
        Response(404);
        Response(500);
    }
}