using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Threads.GetThreadsSummary;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetThreadsSummary";
        Description = """
                      Returns key statistics for the Threads section homepage.

                      Used to display:
                      - "X Threads this week"
                      - "X Threads today"

                      on the main banner "Join The Conversation."
                      """;

        Response<Response>(200, "Threads summary returned successfully.");
        Response(500);
    }
}