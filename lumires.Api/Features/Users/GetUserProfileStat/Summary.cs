using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Users.GetUserProfileStat;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetUserProfileStat";
        Description = """"
                      Gives user`s short statistics from his profile""";

                      Returns 404 Not Found if user`s does not exist.
                      """";
        Response(200, "Users stat is successfully retrieved");
    }
}