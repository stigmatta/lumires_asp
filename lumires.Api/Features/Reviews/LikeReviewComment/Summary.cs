using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.LikeReviewComment;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "LikeReviewComment";
        Description = """
                      Likes a review comment with a current user id.

                      If already liked - remove like.

                      Can return 429 if too more than 4 request were made in a span of 2 seconds from a current user.

                      Returns button state (is liked or not) and likes count

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Query(new Guid("a3f1c9e2-7b4a-4c1f-9d2a-123456789abc"));

        Response(200, "Review is successfully liked");
        Response(404);
        Response(429);
        Response(401);
    }
}