using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Features.FilmsLists.GetFilmsList;

namespace lumires.Api.Features.FilmsLists.GetEditorialPickFilmsLists;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetEditorialPickFilmsLists";
        Description = """
                      Returns films lists picked by editors.

                      Available to anonymous users. Private collections are only visible to their owner.

                      """;

        ExampleRequest = new Query(Guid.NewGuid());

        Response(200, "Collections retrieved successfully.", example: new Response(
        [
            new EditorialListItem(Guid.CreateVersion7(),
                "My favourite movies",
                Guid.CreateVersion7(),
                "username",
                2,
                true,
                [new FilmListItem("some-backdrop"), new FilmListItem("some-other-backdrop")])
        ]));
    }
}