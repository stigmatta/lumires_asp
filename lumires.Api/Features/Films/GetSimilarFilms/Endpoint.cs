using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core;
using lumires.Core.Abstractions.Services;
using lumires.Core.Events.Films;
using lumires.Core.Helpers;

namespace lumires.Api.Features.Films.GetSimilarFilms;

[UsedImplicitly]
internal sealed record Query(int Id);

[UsedImplicitly]
internal sealed record GenreItem(
    int Id,
    string Name);

[UsedImplicitly]
internal sealed record SimilarFilmItem(
    int ExternalId,
    string? PosterPath,
    string Title,
    string Slug,
    int? ReleaseYear,
    GenreItem[] Genres,
    float Rating);

[UsedImplicitly]
internal sealed record Response(IReadOnlyCollection<SimilarFilmItem> Films);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    IExternalFilmService externalFilmService,
    DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/films/{Slug}/{Id:int}/similar");
        Description(x => x.WithTags("Films"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;

        var externalResults = await externalFilmService.GetSimilarFilmsAsync(query.Id, lang, ct);

        if (!externalResults.IsSuccess)
        {
            await HttpContext.SendErrorAsync(externalResults.Status, ct);
            return;
        }

        var externalFilms = externalResults.Value;

        if (externalFilms.Count == 0)
        {
            await Send.OkAsync(new Response([]), ct);
            return;
        }

        var similarFilmsIds = externalFilms.Select(x => x.ExternalId).ToArray();

        var existingFilmsDict = (await db.GetExistingFilms(similarFilmsIds, lang, ct))
            .ToDictionary(x => x.Id);

        var allGenresDict = await db.GetGenresDictionaryAsync(lang, ct);

        var films = externalFilms.Select(external =>
        {
            if (existingFilmsDict.TryGetValue(external.ExternalId, out var local))
            {
                var (rating, _) = CalculateFilmRating.Handle(
                    external.VoteAverage,
                    external.VoteCount,
                    local.VoteAverage,
                    local.VoteCount
                );

                return new SimilarFilmItem(
                    external.ExternalId,
                    external.PosterPath,
                    local.Title,
                    local.Slug,
                    local.ReleaseYear,
                    local.Genres,
                    rating
                );
            }

            var slug = SlugExtensions.Slugify($"{external.Title}-{external.ReleaseYear}");

            var genres = external.GenreIds.Select(id =>
                    allGenresDict.TryGetValue(id, out var genre)
                        ? genre
                        : new GenreItem(id, $"Unknown ({id})"))
                .ToArray();

            return new SimilarFilmItem(
                external.ExternalId,
                external.PosterPath,
                external.Title,
                slug,
                external.ReleaseYear,
                genres,
                external.VoteAverage
            );
        }).ToList();

        await Send.OkAsync(new Response(films), ct);

        var idsToEnrich = externalFilms
            .Where(x => !existingFilmsDict.ContainsKey(x.ExternalId))
            .Select(x => x.ExternalId)
            .ToList();

        if (idsToEnrich.Count > 0)
        {
            await new FilmReferencedEvent
            {
                ExternalIds = [..idsToEnrich],
                Language = lang
            }.PublishAsync(Mode.WaitForNone, CancellationToken.None);

            await new FilmEnrichmentEvent
            {
                ExternalIds = [..idsToEnrich],
                SkipLanguage = lang
            }.PublishAsync(Mode.WaitForNone, CancellationToken.None);
        }
    }
}