using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmPeople.GetDirector;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<Response?> GetDirectorByIdAsync(int tmdbId, string lang, CancellationToken ct)
    {
        return await db.PersonsDetails
            .AsNoTracking()
            .Where(pd => pd.Person.ExternalId == tmdbId
                         && pd.Person.PersonDepartment == PersonDepartment.Directing)
            .Where(pd => pd.LanguageCode == lang || pd.LanguageCode == DefLang)
            .OrderByDescending(pd => pd.LanguageCode == lang)
            .Select(m => new Response(
                m.Person.ExternalId,
                m.LanguageCode,
                m.Person.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Name)
                    .FirstOrDefault() ?? "Unknown",
                m.Biography,
                m.Birthday,
                m.Deathday,
                m.Gender,
                m.PlaceOfBirth,
                m.ProfilePath
            ))
            .FirstOrDefaultAsync(ct);
    }
}