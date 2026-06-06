using Ardalis.Result;
using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Relationships.FollowUser;

[UsedImplicitly]
internal sealed record Command(Guid TargetUserId);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess db)
    : Endpoint<Command, EmptyResponse>
{
    public override void Configure()
    {
        Post("/users/{targetUserId}/follow");
        Description(x => x.WithTags("Relationships"));
        Throttle(5, 2);
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;

        if (currentUserId == command.TargetUserId)
        {
            await Send.NoContentAsync(ct);
            return;
        }

        var currentUsername = await currentUserService.GetUsernameAsync(ct);

        var result = await db.FollowUserAsync(command, currentUserId, currentUsername, ct);

        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        switch (result.Status)
        {
            //New relationship
            case ResultStatus.Created:
                await Send.StatusCodeAsync(StatusCodes.Status201Created, ct); //TODO maybe change
                break;
            //Nothing changed
            default:
                await Send.NoContentAsync(ct);
                break;
        }
    }
}