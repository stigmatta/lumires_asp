using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Features.Genres.Contracts;

namespace lumires.Api.Features.Genres.GetGenres;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetGenres";
        Description = """
                      Returns DTO for list of all genres

                      ### Headers

                      - **Accept-Language** — Preferred language (e.g., `uk-UA`, `en-US`)
                      """;

        Response(200, "Genres retrieved successfully.", example: new Response(
            new List<GenreItem>([new GenreItem(15, "Action")])
        ));
        Response(500);
    }
}