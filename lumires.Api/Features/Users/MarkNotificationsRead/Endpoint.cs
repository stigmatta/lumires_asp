using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Users.MarkNotificationsRead;

[UsedImplicitly]
internal sealed record Command(Guid[] Ids);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess)
    : Endpoint<Command, EmptyResponse>
{
    public override void Configure()
    {
        Post("/users/{username}/notifications/read");
        Description(x => x.WithTags("Users"));
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;

        await dataAccess.MarkNotificationsRead(command, currentUserId, ct);

        await Send.NoContentAsync(ct);
    }
}
