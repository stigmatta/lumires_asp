using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Settings.UpdateProfileSettings;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "UpdateProfileSettings";
        Description = """
                      Updates profile settings of self.

                      Can return 403 if jwt is not corresponding with a current user

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        Response(204, "Profile settings are successfully updated");
        Response(400);
    }
}