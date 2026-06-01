using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Enums.Common;

namespace lumires.Api.Features.FilmsLists.GetFilmsLists;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetFilmsLists";
        Description = """
                      Get lists sorted & filtered & paginated.

                      Returns the reviews DTO and pagination info.

                      """;

        ExampleRequest = new Query
        {
            Category = ContentFilterEnum.All,
            SortBy = ListContentOrderEnum.MostRecent,
            Page = 1,
            PageSize = 5
        };


        Response(200, "Review is successfully retrieved");
    }
}