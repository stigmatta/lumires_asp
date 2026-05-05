using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Collections.Commands.CreateCollection;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "CreateCollection";
        Description = """
                      Creates a new collection for the authenticated user.

                      Optionally accepts a list of movie IDs to add to the collection on creation.

                      ### Body

                      - **Title** — Collection title
                      - **Description** — Optional description
                      - **IsPrivate** — Whether the collection is private
                      - **MovieIds** — Optional list of TMDB movie IDs to add
                      """;

        ExampleRequest = new Command(
            "My Favourite Movies",
            "A list of movies I love.",
            false,
            []
        );

        Response(201, "Collection created successfully.", example: new Response(
            Guid.NewGuid(),
            "My Favourite Movies",
            DateTimeOffset.UtcNow
        ));
        Response(400);
        Response(401);
        Response(500);
    }
}