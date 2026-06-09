using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Features.Films.Contracts;
using lumires.Core;
using lumires.Core.Abstractions.Services;
using lumires.Core.Helpers;
using lumires.Domain.Enums;

namespace lumires.Api.Features.Films.GetDirectorFilmography;

[UsedImplicitly]
internal sealed record Query(int Id);

[UsedImplicitly]
internal sealed record Response(IReadOnlyCollection<CommonFilmListResponse> Films);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    IExternalFilmService externalFilmService,
    IPersonResolver personResolver,
    DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/directors/{Id:int}/filmography");
        Description(x => x.WithTags("Films"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;

        await personResolver.EnsurePersonExistsAsync((query.Id, nameof(PersonDepartment.Directing)), lang, ct);

        var externalResults = await externalFilmService.GetPersonCreditsAsync(query.Id, lang, ct);

        if (!externalResults.IsSuccess)
        {
            await HttpContext.SendErrorAsync(externalResults.Status, ct);
            return;
        }

        var directorFilms = externalResults.Value.AsDirector;

        if (directorFilms.Count == 0)
        {
            await Send.OkAsync(new Response([]), ct);
            return;
        }

        var directorFilmsIds = directorFilms.Select(x => x.ExternalId).ToArray();

        var existingFilmsDict = (await db.GetExistingFilms(directorFilmsIds, lang, ct))
            .ToDictionary(x => x.Id);

        var allGenresDict = await db.GetGenresDictionaryAsync(lang, ct);

        var films = directorFilms.Select(external =>
        {
            if (existingFilmsDict.TryGetValue(external.ExternalId, out var local))
            {
                var (rating, _) = CalculateFilmRating.Handle(
                    external.VoteAverage,
                    external.VoteCount,
                    local.VoteAverage,
                    local.VoteCount
                );

                return new CommonFilmListResponse(
                    external.ExternalId,
                    local.Title,
                    external.PosterPath,
                    local.ReleaseYear,
                    local.Genres,
                    rating
                );
            }

            var genres = external.GenreIds
                .Where(allGenresDict.ContainsKey)
                .Select(id => allGenresDict[id])
                .ToArray();

            return new CommonFilmListResponse(
                external.ExternalId,
                external.Title,
                external.PosterPath,
                external.ReleaseYear,
                [.. genres.Select(g => g.Name)],
                external.VoteAverage
            );
            
        }).ToList();

        await Send.OkAsync(new Response(films), ct);
    }
}