using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Threads.GetThread;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetThread";
        Description = """
                      Get a thread.

                      Returns the thread and replies to this thread.

                      If the thread was not found - returns 404 Not Found.

                      """;

        ExampleRequest = new Query(
            new Guid("7e432661-94e2-474d-b0de-ef2d83005791")
        );
        Response(200, "Thread is successfully retrieved");
        Response(404);
    }
}