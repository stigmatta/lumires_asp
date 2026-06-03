using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Relationships.UnblockUser;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "UnblockUser";
        Description = """
                      Unblocks a specific user

                      If the user was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(Guid.CreateVersion7());
        Response(204);
        Response(400);
        Response(404);
        Response(409);
        Response(403);
    }
}