using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.UpdateReviewComment;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "UpdateReviewComment";
        Description = """
                      Updates a comment for a specific review.

                      Returns 204 No content.

                      If the review was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      - **TargetedUserId** — Optional, used when replying to a specific comment
                      """;

        ExampleRequest = new Command(
            new Guid("a3f1c9e2-7b4a-4c1f-9d2a-123456789abc"),
            Guid.CreateVersion7(),
            "Totally agree with your take on the third act.",
            null,
            true
        );

        Response(204, "Comment is successfully review");
        Response(400);
        Response(403);
        Response(404);
    }
}