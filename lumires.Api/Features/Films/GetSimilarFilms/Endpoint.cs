using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Core.Events.Films;
using lumires.Core.Models;

namespace lumires.Api.Features.Films.GetSimilarFilms;

[UsedImplicitly]
internal sealed record Query(int Id);

[UsedImplicitly]
internal sealed record Response(IReadOnlyCollection<ExternalFilmShort> Films);

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

        var response = await externalFilmService.GetSimilarFilmsAsync(query.Id, lang, ct);

        if (!response.IsSuccess)
        {
            await HttpContext.SendErrorAsync(response.Status, ct);
            return;
        }

        var similarFilmsIds = response.Value.Select(x => x.ExternalId).ToArray();
        if (similarFilmsIds.Length == 0)
        {
            await Send.OkAsync(new Response([]), ct);
            return;
        }

        var existingIds = await db.GetExistingFilms(similarFilmsIds, ct) ?? [];
        var idsToEnrich = similarFilmsIds.Except(existingIds).ToList();

        await Send.OkAsync(new Response(response.Value), ct);

        if (idsToEnrich.Count == 0) return;

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