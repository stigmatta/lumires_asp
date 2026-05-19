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

        var details = await db.PersonsDetails
            .AsNoTracking()
            .Where(pd => pd.Person.ExternalId == tmdbId 
                         && pd.Person.PersonDepartment == PersonDepartment.Directing)
            .Where(pd => pd.LanguageCode == lang 
                         || pd.LanguageCode == DefLang)
            .Select(m => new Response(
                m.Person.ExternalId,
                m.LanguageCode,
                m.Biography,
                m.Birthday,
                m.Deathday,
                m.Gender,
                m.PlaceOfBirth,
                m.ProfilePath
            ))
            .ToListAsync(ct);
        
        var exact = details.FirstOrDefault(d => d.Lang == lang);
        return exact ?? details.FirstOrDefault(d => d.Lang == DefLang);
    }
}