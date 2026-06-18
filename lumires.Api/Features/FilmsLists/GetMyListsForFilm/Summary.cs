using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmsLists.GetMyListsForFilm;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetMyListsForFilm";
        Description = "Returns all film lists belonging to the current user for a given film, including whether the film is in each list.";
        Response(200, "Lists returned successfully");
    }
}
