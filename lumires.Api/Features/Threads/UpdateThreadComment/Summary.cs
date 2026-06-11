using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Threads.UpdateThreadComment;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "UpdateThreadComment";
        Description = """
                      Updates a comment for a specific thread.

                      Returns No Content 204.

                      If the thread was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.
                      
                      If its not yours - 403 Forbidden

                      ### Notes

                      - **Authorization Bearer** — Is required
                      - **TargetedUserId** — Optional, used when replying to a specific comment
                      """;

        ExampleRequest = new Command(
            Guid.CreateVersion7(),
            new Guid("a3f1c9e2-7b4a-4c1f-9d2a-123456789abc"),
            "Totally agree with your take on the third act.",
            null
        );

        Response(204, "Comment is successfully updated");
        Response(400);
        Response(403);
        Response(404);
    }
}