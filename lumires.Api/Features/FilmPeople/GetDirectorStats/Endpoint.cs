using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Enums;

namespace lumires.Api.Features.FilmPeople.GetDirectorStats;

[UsedImplicitly]
internal sealed record Query(int Id);

[UsedImplicitly]
internal sealed record AwardsResponse(int Nominations, int Wins);

[UsedImplicitly]
internal sealed record Response(
    int DirectorId,
    int FilmsCount,
    double AverageRating,
    AwardsResponse? Awards);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    IExternalFilmService externalFilmService,
    IExternalAwardsService externalAwardsService,
    IPersonResolver personResolver)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/directors/{Id:int}/stats");
        Description(x => x.WithTags("People"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;

        await personResolver.EnsurePersonExistsAsync((query.Id, nameof(PersonDepartment.Directing)), lang, ct);

        var creditsResult = await externalFilmService.GetPersonCreditsAsync(query.Id, lang, ct);

        if (!creditsResult.IsSuccess)
        {
            await HttpContext.SendErrorAsync(creditsResult.Status, ct);
            return;
        }

        var directorFilms = creditsResult.Value.AsDirector;

        var ratedFilms = directorFilms
            .Where(f => f.VoteCount > 0)
            .Select(f => f.VoteAverage)
            .ToList();

        // TMDB vote averages are on a 0–10 scale; the app stores ratings on 0–5.
        var averageRating = ratedFilms.Count > 0
            ? Math.Round(ratedFilms.Average() / 2.0, 1)
            : 0;

        // TMDB has no awards API; scraping the website is best-effort and must not
        // fail the whole response. Awards are null when unavailable.
        var awardsResult = await externalAwardsService.GetPersonAwardsAsync(query.Id, ct);
        var awards = awardsResult.IsSuccess
            ? new AwardsResponse(awardsResult.Value.Nominations, awardsResult.Value.Wins)
            : null;

        var response = new Response(
            query.Id,
            directorFilms.Count,
            averageRating,
            awards);

        await Send.OkAsync(response, ct);
    }
}
