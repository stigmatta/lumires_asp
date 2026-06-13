using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetUserFeaturedReview;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetUserFeaturedReview";
        Description = """
                      Get user the most popular review.

                      Can return 204 No Content if no review.
                      
                      ### Notes

                      - **Language header**
                      """;

        ExampleRequest = new Query("username");

        Response(200, "Review is retrieved");
        Response(204);
    }
}