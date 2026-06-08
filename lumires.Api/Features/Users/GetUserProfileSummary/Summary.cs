using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Users.GetUserProfileSummary;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetUserProfileSummary";
        Description = """"
                      Gives user`s summary from his profile.""";

                      Returns 404 Not Found if user`s does not exist.
                      """";
        Response(200, "Users summary is successfully retrieved");
    }
}