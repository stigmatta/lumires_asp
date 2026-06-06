using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Users.GetThisWeekMostActiveUsers;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetThisWeekMostActiveUsers";
        Response(200, "Users are successfully retrieved");
    }
}