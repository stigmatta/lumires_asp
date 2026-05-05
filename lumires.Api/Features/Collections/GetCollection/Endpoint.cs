using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Collections.GetCollection;

[UsedImplicitly]
internal sealed record Query(Guid Id);

[UsedImplicitly]
internal sealed record Response(
    Guid Id,
    string Title,
    string? Description,
    string AuthorName,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<CollectionMovieItem> Movies);

[UsedImplicitly]
internal sealed record CollectionMovieItem(
    Guid MovieId,
    string Title,
    string? PosterPath,
    int Order);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess) : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("collections/{id}");
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

        var result = await dataAccess.GetCollectionAsync(id, lang, ct);

        if (result is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}