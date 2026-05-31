using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Threads.CreateThread;

[UsedImplicitly]
internal sealed record Command(string? Title, string Text, bool IsSpoilerFree = true);

[UsedImplicitly]
internal sealed record Response(
    Guid Id,
    string? Title,
    string Text,
    DateOnly CreatedAt,
    bool IsSpoilerFree
);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess)
    : Endpoint<Command, Response>
{
    public override void Configure()
    {
        Post("/threads");
        Description(x => x.WithTags("Threads"));
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;

        var result = await dataAccess.CreateThreadAsync(command, currentUserId, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        var response = new Response(
            result.Value,
            command.Title,
            command.Text,
            DateOnly.FromDateTime(DateTime.UtcNow),
            command.IsSpoilerFree
        );
        await Send.CreatedAtAsync<GetThread.Endpoint>(
            new { id = response.Id },
            response,
            cancellation: ct);
    }
}