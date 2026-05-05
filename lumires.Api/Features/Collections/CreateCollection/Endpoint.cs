using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Collections.CreateCollection;

[UsedImplicitly]
internal sealed record Command(
    string Title,
    string? Description,
    bool IsPrivate,
    IReadOnlyCollection<Guid> MovieIds
);

[UsedImplicitly]
internal sealed record Response(Guid CollectionId, string Title, DateTimeOffset CreatedAt);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess)
    : Endpoint<Command, Response>
{
    public override void Configure()
    {
        Post("/collections/");
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;

        var collectionId = await dataAccess.CreateCollectionAsync(command, currentUserId, ct);

        var response = new Response(
            collectionId,
            command.Title,
            DateTimeOffset.UtcNow
        );
        await Send.CreatedAtAsync<GetCollection.Endpoint>(
            new { id = collectionId },
            response,
            cancellation: ct);
    }
}