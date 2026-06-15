using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.FilmsLists.GetMyListsForFilm;

[UsedImplicitly]
internal sealed record Query(int FilmId);

[UsedImplicitly]
internal sealed record FilmListItem(string? PosterPath);

[UsedImplicitly]
internal sealed record ListResponse(
    Guid Id,
    string Title,
    int FilmsCount,
    bool IsPrivate,
    bool ContainsFilm,
    IReadOnlyCollection<FilmListItem> Films);

[UsedImplicitly]
internal sealed record Response(IReadOnlyCollection<ListResponse> Lists);

internal sealed class Endpoint(DataAccess db, ICurrentUserService currentUserService)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/films/{filmId:int}/lists/mine");
        Description(x => x.WithTags("Lists"));
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var userId = currentUserService.UserId;

        var response = await db.GetListsAsync(query, userId, ct);

        await Send.OkAsync(response, ct);
    }
}
