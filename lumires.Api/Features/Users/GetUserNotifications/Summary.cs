using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Users.GetUserNotifications;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetUserNotifications";
        Description = """"
                      Gives user`s notifications""";

                      Returns 404 Not Found if user`s does not exist.
                      """";
        Response(200, "User`s notifications are successfully retrieved");
        Response(404);
    }
}