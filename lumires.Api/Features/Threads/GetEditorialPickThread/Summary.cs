using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Features.FilmsLists.GetFilmsList;

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

        ExampleRequest = new Query(Guid.NewGuid());

        Response(200, "Thread successfully retrieved");
    }
}