using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Settings.DeleteAccount;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "DeleteAccount";
        Description = """
                      Deletes self-account.

                      Can return 403 if jwt is not corresponding with a current user

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        Response(204, "Account is successfully deleted");
        Response(400);
    }
}