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

        ExampleRequest = new Query(new Guid("a3f1c9e2-7b4a-4c1f-9d2a-123456789abc"));

        Response(200, "Review is retrieved");
        Response(204);
    }
}