using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Enums.Common;

namespace lumires.Api.Features.Reviews.GetUserLikedReviews;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetUserLikedReviews";
        Description = """
                      Get user liked reviews sorted & filtered & paginated.

                      Returns the reviews DTO and pagination info.

                      """;

        ExampleRequest = new Query
        {
            Filter = RatingEnum.All,
            SortBy = ContentOrderEnum.MostRecent,
            Page = 1,
            PageSize = 5
        };


        Response(200, "Reviews are successfully retrieved");
    }
}