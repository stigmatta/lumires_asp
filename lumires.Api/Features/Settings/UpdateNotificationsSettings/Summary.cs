using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Settings.UpdateNotificationsSettings;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "UpdateNotificationsSettings";
        Description = """
                      Updates notification settings of self.

                      Can return 403 if jwt is not corresponding with a current user

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        Response(204, "Notification settings are successfully updated");
        Response(400);
    }
}