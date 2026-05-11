using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetReviewsByMoviePreview;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetReviewsByMoviePreview";
        Description =
            Description = """
                          Get top 4 popular reviews for a specific movie with the most liked reply to each.
                          If movie doesnt exist - returns empty collection.

                          """;
        Response(200, "Reviews preview successfully retrieved");
        Response(404);
    }
}