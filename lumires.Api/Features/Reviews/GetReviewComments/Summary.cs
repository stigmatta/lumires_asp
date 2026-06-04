using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetReviewComments;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetReviewComments";
        Description = "Get review comments for a specific comments paginated and ordered by creation date";

        Response(200, "Review comments are successfully retrieved");
    }
}