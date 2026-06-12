using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.FilmsLists.UpdateFilmsList;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "UpdateFilmsList";
        Description = """
                      Updates a films list for the authenticated user.

                      Optionally accepts a list of movie IDs to add to the collection on creation.

                      ### Body

                      - **Title** — Collection title
                      - **Description** — Optional description
                      - **IsPrivate** — Whether the collection is private
                      - **MovieIds** — Optional list of TMDB movie IDs to add
                      """;

        ExampleRequest = new Command(
            Guid.CreateVersion7(),
            "My Favourite Movies",
            "A list of movies I love.",
            false,
            []
        );

        Response(204, "List is updated");
        Response(400);
        Response(401);
        Response(403);
        Response(500);
    }
}