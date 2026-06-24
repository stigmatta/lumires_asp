using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Settings.UpdateAccentTheme;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "UpdateAccentTheme";
        Description = """
                      Updates the accent theme of self.

                      Can return 403 if jwt is not corresponding with a current user.

                      Pass null to reset to the default theme.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        Response(204, "Accent theme is successfully updated");
        Response(400);
    }
}
