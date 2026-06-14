using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Threads.GetEditorialPickThread;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetEditorialPickThread";
        Description = """
                          Returns the most popular thread picked by editors.
                          
                          Returns 204 No Content if no thread
                      """;

        Response(200, "Thread successfully retrieved");
    }
}