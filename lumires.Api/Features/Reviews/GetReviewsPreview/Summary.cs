using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetReviewsPreview;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetReviewsPreview";
        Description = "Get reviews preview for a main page.";


        Response(200, "Reviews are successfully retrieved");
    }
}