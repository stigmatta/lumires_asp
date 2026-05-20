using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetReviewsSummary;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetReviewsSummary";
        Description = """
                      Returns key statistics for the Reviews section homepage.

                      Used to display:
                      - "X Reviews this week"
                      - "X Reviews today"

                      on the main banner "Read The Reviews."
                      """;

        Response<Response>(200, "Reviews summary returned successfully.");
        Response(500);
    }
}