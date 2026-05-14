using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmsLists.GetFilmsListsByFilm;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetFilmsListsByFilm";
        Description = """
                      Returns four the most popular collections with the film, which id was given.

                      Available to anonymous users. 

                      ### Route parameters

                      - **Id** — Film identifier

                      """;

        ExampleRequest = new Query(550);

        Response(200, "Collections retrieved successfully.", example: new Response(
        [
            new FilmsListsItems([new FilmListItem("some-backdrop"), new FilmListItem("some-other-backdrop")],
                "My favourite movies")
        ]));
    }
}