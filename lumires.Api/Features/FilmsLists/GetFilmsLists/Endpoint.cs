using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Core.Models;

namespace lumires.Api.Features.FilmsLists.GetFilmsLists;

[UsedImplicitly]
internal enum ContentFilterEnum
{
    All,
    Trending,
    RecentlyUpdated,
    EditorPicks,
    NewLists,
    FriendsLists
}

[UsedImplicitly]
internal enum ListContentOrderEnum
{
    MostRecent,
    MostPopular,
    MostFilms
}

[UsedImplicitly]
internal sealed class Query
{
    public ContentFilterEnum? Category { get; init; } = ContentFilterEnum.All;
    public ListContentOrderEnum? SortBy { get; init; } = ListContentOrderEnum.MostRecent;
    public int? FilmId { get; init; }
    public Guid? UserId { get; init; } //if requested from profile
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 6;
}

[UsedImplicitly]
internal sealed record ListItemResponse(
    Guid Id,
    string Title,
    Guid UserId,
    string Username,
    int FilmsCount,
    bool IsLikedByMe,
    bool IsSavedByMe,
    bool IsPrivate,
    bool IsMyList,
    IReadOnlyCollection<FilmListItem> Films
);

[UsedImplicitly]
internal sealed record FilmListItem(string? BackdropPath);

internal sealed class Endpoint(DataAccess db, ICurrentUserService currentUserService)
    : Endpoint<Query, PagedResponse<ListItemResponse>>
{
    public override void Configure()
    {
        Get("/lists");
        Description(x => x.WithTags("Lists"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var userId = currentUserService.UserId;

        var response = await db.GetListsAsync(query, userId, ct);
        var count = await db.GetListsCountAsync(query, userId, ct);

        var paged = new PagedResponse<ListItemResponse>(
            response,
            count,
            query.Page,
            query.PageSize
        );

        await Send.OkAsync(paged, ct);
    }
}