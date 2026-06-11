using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Threads.DeleteThread;

[UsedImplicitly]
internal sealed record Command(Guid ThreadId);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess)
    : Endpoint<Command, EmptyResponse>
{
    public override void Configure()
    {
        Delete("/threads/{threadId:guid}");
        Description(x => x.WithTags("Threads"));
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var userRole = currentUserService.UserRole;

        var result = await dataAccess.DeleteThreadAsync(command, currentUserId, userRole, ct);

        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}