using FastEndpoints;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Events.Films;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Services;

internal class FilmResolver(IAppDbContext db) : IFilmResolver, IResolver
{
    public async Task<bool> EnsureFilmExistsAsync(
        int externalId,
        string language,
        CancellationToken ct)
    {
        var film = await db.Films
            .Include(p => p.Localizations)
            .FirstOrDefaultAsync(m => m.ExternalId == externalId, ct);

        if (film is not null && film.Localizations.Any(d => d.LanguageCode == language))
            return true;

        await new FilmReferencedEvent
        {
            ExternalIds = [externalId],
            Language = language
        }.PublishAsync(Mode.WaitForAll, ct);

        return false;
    }
    
    public async Task EnsureFilmsExistAsync(
        IReadOnlyCollection<int> externalIds,
        string language,
        CancellationToken ct)
    {
        var existingIds = await db.Films
            .Where(f => externalIds.Contains(f.ExternalId))
            .Where(f => f.Localizations.Any(l => l.LanguageCode == language))
            .Select(f => f.ExternalId)
            .ToListAsync(ct);

        var missingIds = externalIds
            .Except(existingIds)
            .ToArray();

        if (missingIds.Length == 0)
            return;

        await new FilmReferencedEvent
        {
            ExternalIds = missingIds,
            Language = language
        }.PublishAsync(Mode.WaitForAll, ct);

        await new FilmEnrichmentEvent
        {
            ExternalIds = missingIds,
            SkipLanguage = language
        }.PublishAsync(Mode.WaitForNone, ct);
    }
}