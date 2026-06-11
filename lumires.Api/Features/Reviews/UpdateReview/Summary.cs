using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.UpdateReview;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "CreateReview";
        Description = """
                      Creates a review for a specific movie.

                      Returns 204 No Content.

                      If the movie or review was not found - returns 404 Not Found.
                      
                      If its not yours review - returns 403 Forbidden

                      If some of the fields are not valid - returns 400 Bad Request.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(
            550,
            Guid.CreateVersion7(),
            "My thoughts on Inception",
            "A mind-bending masterpiece that challenges the boundaries of reality.",
            5f
        );
        Response(204, "Review is successfully updated");
        Response(400);
        Response(403);
        Response(404);
    }
}