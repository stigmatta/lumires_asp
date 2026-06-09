using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmsLists.GetUserLikedFilmsLists;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetUserLikedFilmsLists";
        Description = """
                      Get user liked lists sorted.

                      Returns the reviews DTO and pagination info.

                      """;

        ExampleRequest = new Query
        {
            SortBy = ListContentOrderEnum.MostRecent,
            Page = 1,
            PageSize = 5
        };
        Response(200, "Collection retrieved successfully.");
        //     , example: new Response(
        //     Guid.NewGuid(),
        //     "My Favourite Movies",
        //     "A list of movies I love.",
        //     "morrigun01",
        //     DateTimeOffset.UtcNow,
        //     true,
        //     false,
        //     [
        //         new ListItemResponse(550, "Fight Club", "/pB8BM7pdSp6B6Ih7QZ4DrQ3PmJK.jpg", 1),
        //         new ListItemResponse(551, "Inception", "/9gk7adHYeDvHkCSEqAvQNLV5Uge.jpg", 2)
        //     ]
        // ));
    }
}