using Infrastructure.Services.Tmdb.Models;
using lumires.Core.Models;
using lumires.Domain.Enums;

namespace Infrastructure.Services.Tmdb.TmdbPerson;

internal static class TmdbPersonMapper
{
    public static ExternalPerson ToDomain(TmdbPersonDetailResponse tmdb)
    {
        return new ExternalPerson(
            tmdb.Id,
            tmdb.Name,
            tmdb.Biography,
            tmdb.Birthday,
            tmdb.Deathday,
            (GenderType)tmdb.Gender,
            tmdb.PlaceOfBirth,
            tmdb.ProfilePath,
            tmdb.KnownForDepartment
        );
    }
}