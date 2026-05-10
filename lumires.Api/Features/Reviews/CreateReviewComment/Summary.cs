using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.CreateReviewComment;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "CreateReviewComment";
        Description = """
                      Creates a comment for a specific review.

                      Returns the created comment DTO.

                      If the review was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      - **TargetedUserId** — Optional, used when replying to a specific comment
                      """;

        ExampleRequest = new Command(
            new Guid("a3f1c9e2-7b4a-4c1f-9d2a-123456789abc"),
            "Totally agree with your take on the third act.",
            null
        );

        Response(201, "Comment is successfully created");
        Response(400);
        Response(404);
    }
}