using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Auth.Queries.GetMe;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetMe";
        Description = """
                      Validates JWT, checks consistency (if user exists not only in the supabase, but in the DB)

                      Returns a shortened user DTO.

                      If user was not found in the DB - return 403 forbidden.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        Response(200, "User is authenticated and retrieved.", example: new Response(
            new Guid("28f619dc-f235-4232-b580-e71b6481109c"),
            "morrigun0@gmail.com",
            "morrigun0",
            "/pB8BM7pdSp6B6Ih7QZ4DrQ3PmJK.jpg"
        ));
        Response(401);
        Response(403);
    }
}