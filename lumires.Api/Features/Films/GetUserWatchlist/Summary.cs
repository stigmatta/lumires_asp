using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Enums.Common;

namespace lumires.Api.Features.Films.GetUserWatchlist;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetUserWatchlist";
        Description = """
                      Get watchlist sorted, filtered & paginated.

                      You can pass or not filters and sorting options.

                      Also you can pass them in any casing (lowercased or uppercased)

                      """;

        ExampleRequest = new Query
        {
            Rating = RatingEnum.All,
            Genres = ["action,thriller"],
            SortBy = FilmContentOrder.LeastRated,
            Page = 1,
            PageSize = 5
        };


        Response(200, "Films are successfully retrieved");
    }
}