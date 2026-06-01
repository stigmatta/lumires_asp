using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmsLists.GetFilmsListsSummary;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetFilmsListsSummary";
        Description = """
                      Returns key statistics for the Lists section homepage.

                      Used to display:
                      - "X Lists"
                      - "X Lists today"

                      on the main banner "Browse The Lists."
                      """;

        Response<Response>(200, "Lists summary returned successfully.");
        Response(500);
    }
}