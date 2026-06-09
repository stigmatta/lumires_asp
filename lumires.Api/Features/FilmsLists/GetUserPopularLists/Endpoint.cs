using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.FilmsLists.GetUserPopularLists;

[UsedImplicitly]
internal sealed record Query(string Username);

[UsedImplicitly]
internal sealed record FilmListItem(string? PosterPath);

[UsedImplicitly]
internal sealed record ListResponse(
    Guid Id,
    string Title,
    int FilmCount,
    bool IsLiked,
    bool IsSaved,
    Guid UserId,
    string Username,
    IReadOnlyCollection<FilmListItem> Films);

[UsedImplicitly]
internal sealed record Response(IReadOnlyCollection<ListResponse> Lists);

internal sealed class Endpoint(DataAccess db, ICurrentUserService currentUserService)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/users/{username}/popular-lists");
        Description(x => x.WithTags("Lists"));
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var userId = currentUserService.UserId;

        var response = await db.GetListsAsync(query, userId, ct);

        if (response is null)
        {
            await Send.NoContentAsync(ct);
            return;
        }
        await Send.OkAsync(response, ct);
    }
    
}