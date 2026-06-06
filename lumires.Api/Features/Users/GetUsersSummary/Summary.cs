using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Users.GetUsersSummary;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetUsersSummary";
        Description = "Gives total members in the system and recently online ones.";
        Response(200, "Users summary is successfully retrieved");
    }
}