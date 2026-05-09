using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetReviews;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetReviews";
        Description = """
                      Get reviews for a specific movie sorted & filtered & paginated.

                      Returns the reviews DTO and pagination info.

                      """;

        ExampleRequest = new Query
        {
            MovieId = new Guid("019cd3eb-ea03-7c41-a0a8-769f4ea68d67"),
            Filter = FilterEnum.All,
            SortBy = SortEnum.MostRecent,
            Page = 1,
            PageSize = 5
        };


        Response(200, "Review is successfully retrieved");
    }
}