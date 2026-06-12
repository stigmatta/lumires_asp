using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.DeleteReviewComment;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "DeleteReviewComment";
        Description = """
                      Deletes a specific review comment.

                      If the review or comment was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.
                      
                      If its not yours - 403 Forbidden

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(Guid.CreateVersion7(), Guid.CreateVersion7());
        Response(204, "Review comment is successfully deleted");
        Response(400);
        Response(403);
        Response(404);
    }
}