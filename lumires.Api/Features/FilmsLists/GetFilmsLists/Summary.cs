using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Enums.Common;
using lumires.Api.Features.FilmsLists.GetFilmsList;

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
        Response(200, "Collection retrieved successfully.", example: new Response(
            Guid.NewGuid(),
            "My Favourite Movies",
            "A list of movies I love.",
            "morrigun01",
            DateTimeOffset.UtcNow,
            true,
            [
                new ListFilmItem(550, "Fight Club", "/pB8BM7pdSp6B6Ih7QZ4DrQ3PmJK.jpg", 1),
                new ListFilmItem(551, "Inception", "/9gk7adHYeDvHkCSEqAvQNLV5Uge.jpg", 2)
            ]
        ));
    }
}