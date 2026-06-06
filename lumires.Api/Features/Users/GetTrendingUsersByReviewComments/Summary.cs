using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Users.GetTrendingUsersByReviewComments;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetTrendingUsersByReviewComments";
        Response(200, "Users are successfully retrieved");
    }
}