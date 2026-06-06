using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Films.GetTrendingFilms;

[UsedImplicitly]
internal sealed record Response(IReadOnlyList<TrendingItem> Items);

[UsedImplicitly]
internal sealed record TrendingItem(
    Guid Id,
    int FilmId,
    string FilmTitle,
    string FilmSlug,
    string? ReviewTitle,
    float? Rating,
    Guid UserId,
    string Username
);

internal sealed class Endpoint(ICurrentUserService currentUserService, DataAccess db)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/films/trending");
        Description(x => x.WithTags("Films"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;

        var response = await db.GetTrendingFilmsAsync(lang, ct);
        await Send.OkAsync(response, ct);
    }
}