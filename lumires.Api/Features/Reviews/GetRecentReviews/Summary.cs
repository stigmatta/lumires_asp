using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetRecentReviews;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetRecentReviews";
        Description = """
                      Get recent reviews.

                      Language is optional
                      """;

        Response(200, "Reviews are successfully retrieved");
    }
}