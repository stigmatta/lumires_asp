using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetReviewsByFilm;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetReviewsByFilm";
        Description = """
                      Get reviews for a specific film sorted & filtered & paginated.

                      If film doesnt exist - returns empty collection.

                      Returns the reviews DTO and pagination info.

                      """;

        ExampleRequest = new Query
        {
            FilmId = 550,
            Filter = FilterEnum.All,
            SortBy = SortEnum.MostRecent,
            Page = 1,
            PageSize = 5
        };


        Response(200, "Review is successfully retrieved");
        Response(404);
    }
}