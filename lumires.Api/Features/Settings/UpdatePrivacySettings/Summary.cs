using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Settings.UpdatePrivacySettings;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "UpdatePrivacySettings";
        Description = """
                      Updates privacy settings of self.

                      Can return 403 if jwt is not corresponding with a current user

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        Response(204, "Privacy settings are successfully updated");
        Response(400);
    }
}