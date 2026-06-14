using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Core.Models;

namespace lumires.Api.Features.FilmsLists.GetFilmsList;

[UsedImplicitly]
internal sealed class Query
{
    public Guid Id { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

[UsedImplicitly]
internal sealed record ListFilmItem(
    int FilmId,
    string Title,
    string? PosterPath,
    int Order);

[UsedImplicitly]
internal sealed record Response(
    Guid Id,
    string Title,
    Guid UserId,
    string Username,
    DateTime LastActivity,
    bool IsLikedByMe,
    bool IsSavedByMe,
    PagedResponse<ListFilmItem> Films);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess) : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/lists/{id}");
        Description(x => x.WithTags("Lists"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        if (query.Id == Guid.Empty)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var lang = currentUserService.LangCulture;
        var currentUserId = currentUserService.UserId;

        var result = await dataAccess.GetFilmsListAsync(query.Id, lang, currentUserId, query.Page, query.PageSize, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
