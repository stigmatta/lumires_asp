using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Users.MarkNotificationsRead;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "MarkNotificationsRead";
        Description = """
                      Mark user notifications as read

                      If some of the fields are not valid - returns 400 Bad Request.

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command([Guid.CreateVersion7()]);
        Response(294, "Notifications marked as read");
        Response(400);
        Response(404);
    }
}