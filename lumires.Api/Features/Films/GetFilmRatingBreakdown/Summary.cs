using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Films.GetFilmRatingBreakdown;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetFilmRatingBreakdown";
        Description = """
                        Method for retrieving rating breakdown for a specific movie.
                        
                        Data is coming from our db, not TMDB API !! 

                      """;

        ExampleRequest = new Query(1);

        Response(200, "Rating breakdown is successfully retrieved");
    }
}