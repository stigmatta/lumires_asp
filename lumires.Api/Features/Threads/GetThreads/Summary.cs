using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Enums.Common;

namespace lumires.Api.Features.Threads.GetThreads;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetThreads";
        Description = """
                      Get threads sorted & filtered & paginated.

                      Returns the threads DTO and pagination info.

                      """;

        ExampleRequest = new Query
        {
            SortBy = ContentOrderEnum.MostRecent,
            Page = 1,
            PageSize = 5
        };


        Response(200, "Threads are successfully retrieved");
    }
}