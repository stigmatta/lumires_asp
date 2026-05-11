using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.CreateReview;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "CreateReview";
        Description = """
                      Creates a review for a specific movie.

                      Returns the created review DTO.

                      If the movie was not found - returns 404 Not Found.

                      If some of the fields are not valid - returns 400 Bad Request.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(
            550,
            "My thoughts on Inception",
            "A mind-bending masterpiece that challenges the boundaries of reality.",
            5m,
            true
        );
        Response(201, "Review is successfully created");
        Response(400);
        Response(404);
    }
}