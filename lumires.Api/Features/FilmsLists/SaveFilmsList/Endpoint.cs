using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.FilmsLists.SaveFilmsList;

[UsedImplicitly]
internal sealed record Command(Guid ListId);

internal sealed class Endpoint(
    DataAccess dataAccess,
    ICurrentUserService currentUserService)
    : Endpoint<Command, EmptyResponse>
{
    public override void Configure()
    {
        Post("/lists/{listId:guid}/save/");
        Description(x => x.WithTags("Lists"));
        Throttle(5, 2);
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;

        var result = await dataAccess.SaveListAsync(command, currentUserId, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}