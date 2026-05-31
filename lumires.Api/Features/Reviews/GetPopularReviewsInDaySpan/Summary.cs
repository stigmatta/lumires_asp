using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetPopularReviewsInDaySpan;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetPopularReviewsInDaySpan";
        Description = """
                      Get reviews for a specific day span.

                      Returns the reviews DTO.

                      """;

        ExampleRequest = new Query(
            30
        );
        Response(200, "Reviews are successfully retrieved");
    }
}