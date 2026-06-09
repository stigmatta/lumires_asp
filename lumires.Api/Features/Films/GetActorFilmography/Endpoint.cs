using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Features.Films.Contracts;
using lumires.Core.Abstractions.Services;
using lumires.Core.Helpers;
using lumires.Domain.Enums;

namespace lumires.Api.Features.Films.GetActorFilmography;

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
        Get("/actors/{Id:int}/filmography");
        Description(x => x.WithTags("Films"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;

        await personResolver.EnsurePersonExistsAsync((query.Id, nameof(PersonDepartment.Acting)), lang, ct);

        var externalResults = await externalFilmService.GetPersonCreditsAsync(query.Id, lang, ct);

        if (!externalResults.IsSuccess)
        {
            await HttpContext.SendErrorAsync(externalResults.Status, ct);
            return;
        }

        var actorFilms = externalResults.Value.AsActor;

        if (actorFilms.Count == 0)
        {
            await Send.OkAsync(new Response([]), ct);
            return;
        }

        var directorFilmsIds = actorFilms.Select(x => x.ExternalId).ToArray();

        var existingFilmsDict = (await db.GetExistingFilms(directorFilmsIds, lang, ct))
            .ToDictionary(x => x.Id);

        var allGenresDict = await db.GetGenresDictionaryAsync(lang, ct);

        var films = actorFilms.Select(external =>
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