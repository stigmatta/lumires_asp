using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Relationships.GetUserFriends;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetUserFriends";
        Description = """
                      Get user friends

                      Returns 404 if user is not found

                      """;

        Response(200, "Friends are retrieved");
        Response(404);
    }
}