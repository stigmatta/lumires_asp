using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Threads.UpdateThread;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "UpdateThread";
        Description = """
                      Updates a thread.

                      Returns 204 No Content.

                      If some of the fields are not valid - returns 400 Bad Request.
                      
                      If thread is not yours - 403 Forbidden.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(
            Guid.CreateVersion7(),
            "My thoughts on Inception",
            "some-image",
            "A mind-bending masterpiece that challenges the boundaries of reality."
        );
        Response(204, "Thread is successfully updated");
        Response(400);
        Response(403);
        Response(404);
    }
}