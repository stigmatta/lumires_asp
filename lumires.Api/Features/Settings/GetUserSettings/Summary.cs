using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Settings.GetUserSettings;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetUserSettings";
        Description = """
                      Get user settings
                      
                      Returns 404 if user is not found

                      Can return 403 if jwt is not corresponding with a current user

                      """;

        Response(204, "Settings are retrieved");
        Response(400);
    }
}