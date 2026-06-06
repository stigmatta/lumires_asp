using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Threads.CreateThreadComment;

[UsedImplicitly]
internal sealed record Command(Guid ThreadId, string Text, Guid? TargetedUserId, bool IsSpoilerFree = true);

[UsedImplicitly]
internal sealed record Response(
    Guid Id,
    string Text,
    DateTime CreatedAt,
    bool IsSpoilerFree
);

internal sealed class Endpoint(
    DataAccess db)
    : Endpoint<Command, Response>
{
    public override void Configure()
    {
        Post("/threads/{ThreadId}/reply");
        Description(x => x.WithTags("Threads"));
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var result = await db.CreateThreadCommentAsync(command, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.CreatedAtAsync<GetThread.Endpoint>(
            responseBody: result.Value,
            cancellation: ct);
    }
}