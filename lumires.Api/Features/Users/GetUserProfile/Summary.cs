using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Users.GetUserProfile;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetUsersProfile";
        Description = """"
                      Gives user`s details from his profile. Determines if this profile is yourself.""";

                      Returns 404 Not Found if user`s does not exist.
                      """";
        Response(200, "Users summary is successfully retrieved");
    }
}