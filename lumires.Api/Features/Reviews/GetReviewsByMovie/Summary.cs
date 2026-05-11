using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetReviewsByMovie;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetReviewsByMovie";
        Description = """
                      Get reviews for a specific movie sorted & filtered & paginated.

                      If movie doesnt exist - returns empty collection.

                      Returns the reviews DTO and pagination info.

                      """;

        ExampleRequest = new Query
        {
            MovieId = 550,
            Filter = FilterEnum.All,
            SortBy = SortEnum.MostRecent,
            Page = 1,
            PageSize = 5
        };


        Response(200, "Review is successfully retrieved");
        Response(404);
    }
}