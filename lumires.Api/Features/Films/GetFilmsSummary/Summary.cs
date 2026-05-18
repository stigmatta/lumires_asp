using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.GetFilmsSummary;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetFilmsSummary";
        Description = """
                      Returns key statistics for the Films section homepage.

                      Used to display:
                      - "X.Xm Films"
                      - "X Genres"

                      on the main banner "Explore The Films."
                      """;

        Response<Response>(200, "Films summary returned successfully.");
        Response(500);
    }
}