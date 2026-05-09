namespace lumires.Api.Features.Reviews.GetReview;

using FastEndpoints;
using JetBrains.Annotations;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetReview";
        Description = """
                      Get a review for a specific movie.

                      Returns the review DTO.

                      If the movie was not found - returns 404 Not Found.

                      """;
        
        ExampleRequest = new Query(
            new Guid("7e432661-94e2-474d-b0de-ef2d83005791")
        );
        Response(200, "Review is successfully retrieved");
        Response(404);
    }
}