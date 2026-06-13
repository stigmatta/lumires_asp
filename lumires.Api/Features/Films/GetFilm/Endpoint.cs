using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Films.GetFilm;

[UsedImplicitly]
internal sealed record Query(int Id);

[UsedImplicitly]
internal sealed record LocalizationResponse(
    string LanguageCode,
    string Title,
    string? Overview,
    string? Tagline
);

[UsedImplicitly]
internal sealed record GenreItemResponse(
    int Id,
    string Name,
    string LanguageCode
);

[UsedImplicitly]
internal sealed record GenresResponse(
    IReadOnlyCollection<GenreItemResponse> Items
);

[UsedImplicitly]
internal sealed record Response(
    int Id,
    DateOnly? ReleaseDate,
    string? TrailerUrl,
    string? PosterPath,
    string? BackdropPath,
    LocalizationResponse? Localization,
    GenresResponse Genres,
    IReadOnlyCollection<PersonShortItem> Cast,
    IReadOnlyCollection<PersonShortItem> Directors,
    string ProductionCompany,
    int Runtime,
    float VoteAverage,
    int VoteCount,
    bool IsLikedByMe,
    bool IsWatchedByMe,
    float? MyRating
);

[UsedImplicitly]
internal record PersonShortItem(
    string Name,
    int Id
);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    IFilmResolver filmResolver,
    DataAccess dataAccess)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/films/{Id:int}");
        Description(x => x.WithTags("Films"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;

        await filmResolver.EnsureFilmExistsAsync(query.Id, lang, ct);

        var response = await dataAccess.GetFilmByIdAsync(query.Id, lang, ct);

        if (response is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(response, ct);
    }
}