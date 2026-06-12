using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmsLists.GetFilmsListsByFilmPreview;

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
            new FilmsListsItems(Guid.CreateVersion7(),
                true,
                false,
                [new FilmInListItem("some-backdrop"), new FilmInListItem("some-other-backdrop")],
                10,
                "My favourite movies")
        ]));
    }
}