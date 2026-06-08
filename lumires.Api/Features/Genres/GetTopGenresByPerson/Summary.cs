using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Genres.GetTopGenresByPerson;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetTopGenresByPerson";
        Description = """
                      Returns DTO for list of top genres for a person 

                      ### Headers

                      - **Accept-Language** — Preferred language (e.g., `uk-UA`, `en-US`)
                      """;

        Response(200, "Genres retrieved successfully.", example: new Response(
            new List<GenreItem>([new GenreItem(Guid.CreateVersion7(), "Action", "en-UK")])
        ));
        Response(500);
    }
}