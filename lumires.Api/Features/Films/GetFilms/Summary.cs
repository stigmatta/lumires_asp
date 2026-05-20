using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Enums.Common;

namespace lumires.Api.Features.Films.GetFilms;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetFilms";
        Description = """
                      Get films sorted, filtered & paginated.

                      You can pass or not filters and sorting options.

                      Also you can pass them in any casing (lowercased or uppercased)


                      """;

        ExampleRequest = new Query
        {
            Rating = RatingEnum.All,
            Genres = ["action,thriller"],
            Content = FilmContentFilter.Popular,
            SortBy = FilmContentOrder.LeastRated,
            Page = 1,
            PageSize = 5
        };


        Response(200, "Films are successfully retrieved");
    }
}