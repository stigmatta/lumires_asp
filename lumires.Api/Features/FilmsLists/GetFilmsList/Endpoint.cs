using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.FilmsLists.GetFilmsList;

[UsedImplicitly]
internal sealed record Query(Guid Id);

[UsedImplicitly]
internal sealed record Response(
    Guid Id,
    string Title,
    string? Description,
    string AuthorName,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<ListFilmItem> Films);

[UsedImplicitly]
internal sealed record ListFilmItem(
    int FilmId,
    string Title,
    string? PosterPath,
    int Order);

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
        var id = query.Id;
        var lang = currentUserService.LangCulture;

        if (id == Guid.Empty)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var result = await dataAccess.GetFilmsListAsync(id, lang, ct);

        if (result is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}