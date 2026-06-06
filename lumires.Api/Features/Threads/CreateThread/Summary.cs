using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Threads.CreateThread;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "CreateThread";
        Description = """
                      Creates a thread.

                      Returns the created thread DTO.

                      If some of the fields are not valid - returns 400 Bad Request.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(
            "My thoughts on Inception",
            "some-image",
            "A mind-bending masterpiece that challenges the boundaries of reality."
        );
        Response(201, "Thread is successfully created");
        Response(400);
    }
}