using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetReviewsByMoviePreview;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetReviewsByMoviePreview";
        Description = "Returns top 4 reviews for a specific movie.";
        Response(200, "Reviews preview successfully retrieved");
    }
}