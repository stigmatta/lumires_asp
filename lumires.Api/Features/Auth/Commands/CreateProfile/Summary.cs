using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Auth.Commands.CreateProfile;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "CreateProfile";
        Description = """
                      Validates JWT, checks uniqueness, creates

                      Returns a shortened user DTO.

                      If user was found in the DB - returns 409 Conflict.

                      If some of the fields are not valid - returns 400 Bad Request"

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;
        ExampleRequest = new Command(
            new Guid("7e432661-94e2-474d-b0de-ef2d83005791"),
            "valid_username",
            "valid_email@gmail.com"
        );
        Response(201, "User is successfully created", "7e432661-94e2-474d-b0de-ef2d83005791");
        Response(400);
        Response(409);
    }
}