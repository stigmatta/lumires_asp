using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Relationships.BlockUser;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "BlockUser";
        Description = """
                      Blocks a specific user

                      If the user was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(Guid.CreateVersion7());
        Response(202, "Blocked or nothing changed");
        Response(400);
        Response(404);
        Response(409, "Cannot block yourself");
        Response(403);
    }
}