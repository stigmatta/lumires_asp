// Query.cs + Response.cs + Endpoint.cs
using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Genres.GetTopGenresByPerson;

[UsedImplicitly]
internal sealed record Query(int PersonId);

[UsedImplicitly]
internal sealed record GenreItem(
    Guid Id,
    string Name,
    string LanguageCode);

[UsedImplicitly]
internal sealed record Response(IReadOnlyList<GenreItem> Genres);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/persons/{PersonId:int}/genres/top");
        Description(x => x.WithTags("Genres"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;

        var result = await db.GetTopGenresByPersonAsync(query.PersonId, lang, ct);

        if (result is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}