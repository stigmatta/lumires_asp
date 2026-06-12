using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Threads.DeleteThread;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "DeleteThread";
        Description = """
                      Deletes a specific thread.

                      If the thread was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.
                      
                      If its not yours - 403 Forbidden

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(Guid.CreateVersion7());
        Response(204, "Thread is successfully deleted");
        Response(400);
        Response(403);
        Response(404);
    }
}