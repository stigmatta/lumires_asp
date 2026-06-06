using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmsLists.GetThisWeekTrendingFilmsLists;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetThisWeekTrendingFilmsLists";
        Description = """
                      Returns six trending collections with the film poster pathes.

                      Available to anonymous users. 

                      """;

        Response(200, "Collections retrieved successfully.", example: new Response(
        [
            new TrendingListItem(Guid.CreateVersion7(), "Some title", Guid.CreateVersion7(), "username", 25,
                true,
                false,
                [new FilmListItem("some-backdrop"), new FilmListItem("some-other-backdrop")])
        ]));
    }
}