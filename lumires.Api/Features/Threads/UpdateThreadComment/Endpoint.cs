using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Threads.UpdateThreadComment;

[UsedImplicitly]
internal sealed record Command(Guid ThreadId, Guid ReplyId, string Text, Guid? TargetedUserId, bool IsSpoilerFree = true);

internal sealed class Endpoint(
    DataAccess db)
    : Endpoint<Command, EmptyResponse>
{
    public override void Configure()
    {
        Put("/threads/{ThreadId}/replies/{ReplyId}");
        Description(x => x.WithTags("Threads"));
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var result = await db.UpdateThreadCommentAsync(command, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}