using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.GetTrendingFilms;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetTrendingFilms";
        Response(200, "Trending  movies retrieved successfully.");
    }
}