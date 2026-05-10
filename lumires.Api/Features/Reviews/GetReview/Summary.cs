using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetReview;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetReview";
        Description = """
                      Get a review for a specific movie.

                      Returns the review and replies to this review.

                      If the review was not found - returns 404 Not Found.

                      """;

        ExampleRequest = new Query(
            new Guid("7e432661-94e2-474d-b0de-ef2d83005791")
        );
        Response(200, "Review is successfully retrieved");
        Response(404);
    }
}