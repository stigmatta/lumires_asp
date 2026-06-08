using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmPeople.GetSimilarActors;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetSimilarActors";
        Description = """
                      Returns similar actors to a specific director.
                      
                      It is calculating from the matching genres

                      ### Route parameters

                      - **Id** — TMDB person identifier

                      ### Headers

                      - **Accept-Language** — Preferred language culture (e.g., `uk-UA`, `en-US`)
                      """;

        ExampleRequest = new Query(1);
        Response<Response>(200, "Similar actors retrieved successfully.");
    }
}