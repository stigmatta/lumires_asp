using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Relationships.UnfollowUser;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "UnfollowUser";
        Description = """
                      Unfollows a specific user

                      If the user was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.

                      If there are other relation between these two users - you can get 409 if blocked,
                      200 if he was followed too and you`ve followed back and 204 if you`ve already
                      followed each other or you were already followed.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(Guid.CreateVersion7());
        Response(204);
        Response(400);
        Response(404);
        Response(409, "Cannot unfollow yourself");
        Response(403);
    }
}