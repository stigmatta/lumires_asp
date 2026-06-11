using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.FilmsLists.GetFilmsListsByFilmPreview;

[UsedImplicitly]
internal sealed record Query(int Id);

[UsedImplicitly]
internal sealed record Response(IReadOnlyCollection<FilmsListsItems> FilmsLists);

[UsedImplicitly]
internal sealed record FilmsListsItems(
    Guid Id,
    bool IsLikedByMe,
    bool IsSavedByMe,
    IReadOnlyCollection<FilmInListItem> Films,
    int FilmCount,
    string Name);

[UsedImplicitly]
internal sealed record FilmInListItem(string? BackdropPath);

internal sealed class Endpoint(
    DataAccess db,
    ICurrentUserService currentUserService) : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/films/{Id:int}/lists");
        Description(x => x.WithTags("Lists"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var filmId = query.Id;
        var currentUserId = currentUserService.UserId;

        var response = await db.GetFilmListsByFilmIdAsync(filmId, currentUserId, ct);

        await Send.OkAsync(response, ct);
    }
}