using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.FilmsLists.GetEditorialPickFilmsLists;

[UsedImplicitly]
internal sealed record Response(IReadOnlyList<EditorialListItem> Items);

[UsedImplicitly]
internal sealed record EditorialListItem(
    Guid Id,
    string Title,
    Guid UserId,
    string Username,
    int FilmCount,
    bool IsLikedByMe,
    bool IsSavedByMe,
    IReadOnlyCollection<FilmListItem> Films
);

[UsedImplicitly]
internal sealed record FilmListItem(string? BackdropPath);

internal sealed class Endpoint(DataAccess db, ICurrentUserService currentUserService)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/lists/editorial");
        Description(x => x.WithTags("Lists"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;

        var response = await db.GetEditorialListsAsync(currentUserId, ct);
        await Send.OkAsync(response, ct);
    }
}