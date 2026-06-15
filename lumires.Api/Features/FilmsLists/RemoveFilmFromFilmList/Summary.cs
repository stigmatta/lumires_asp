using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmsLists.RemoveFilmFromFilmList;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "RemoveFilmFromFilmList";
        Description = """
                      Removes a film to a film list for the authenticated user.

                      """;


        Response(204);
        Response(400);
        Response(401);
        Response(404);
        Response(500);
    }
}