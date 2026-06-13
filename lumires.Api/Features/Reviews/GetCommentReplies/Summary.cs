using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetCommentReplies;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetCommentReplies";
        Description = "Get the nested replies of a review comment, paginated and ordered by creation date";

        Response(200, "Comment replies are successfully retrieved");
    }
}
