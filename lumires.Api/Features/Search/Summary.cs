using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Search;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "Search for films and reviews";
        Description = """
                      Provides search functionality across films, reviews, and lists.
                      Returns paged results with metadata for client-side navigation.
                      """;

        ExampleRequest = new Query
        {
            Filter = ContentType.All,
            SearchTerm = "Inception",
            Page = 1
        };

        Response(200, "Search results successfully retrieved");
        Response(400, "Invalid search parameters");
    }
}