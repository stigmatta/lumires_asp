using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Enums.Common;

namespace lumires.Api.Features.Users.GetUserWatchlist;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetUserWatchlist";
        Description = """
                      Get a user's watchlist films sorted, filtered & paginated.

                      You can pass or not filters and sorting options.

                      Also you can pass them in any casing (lowercased or uppercased)

                      If the watchlist is private and the requester is not the owner - returns 403 Forbidden.

                      If the user was not found - returns 404 Not Found.
                      """;

        ExampleRequest = new Query
        {
            Rating = RatingEnum.All,
            Genres = ["action,thriller"],
            SortBy = FilmContentOrder.LeastRated,
            Page = 1,
            PageSize = 5
        };


        Response(200, "Watchlist films are successfully retrieved");
        Response(403);
        Response(404);
    }
}
