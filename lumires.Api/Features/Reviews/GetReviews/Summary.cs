using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Enums.Common;

namespace lumires.Api.Features.Reviews.GetReviews;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetReviews";
        Description = """
                      Get reviews for sorted & filtered & paginated.

                      Returns the reviews DTO and pagination info.

                      """;

        ExampleRequest = new Query
        {
            Filter = RatingEnum.All,
            SortBy = ContentOrderEnum.MostRecent,
            Page = 1,
            PageSize = 5
        };


        Response(200, "Review is successfully retrieved");
    }
}