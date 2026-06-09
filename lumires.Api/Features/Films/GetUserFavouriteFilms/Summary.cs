using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.GetUserFavouriteFilms;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetUserFavouriteFilms";
        Response(200, "Favourite films are successfully retrieved");
    }
}