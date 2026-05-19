using System.Net;
using Ardalis.Result;
using Infrastructure.Services.Tmdb.Models;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Core.Models;
using lumires.Domain.Enums;

namespace Infrastructure.Services.Tmdb;

public sealed class TmdbPersonService(
    ITmdbApi tmdbApi) : IExternalPersonService
{ 
    private const string DefLang = LocalizationConstants.DefaultCulture;
    
    public async Task<Result<ExternalPerson>> GetPersonDetailsAsync(int personId, string lang, CancellationToken ct)
    {
        var tmdbResponse = await tmdbApi.GetPersonDetailsAsync(personId, lang, ct);

        switch (tmdbResponse.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                return Result.Unauthorized();
            case HttpStatusCode.NotFound:
                return Result.NotFound();
        }

        if (!tmdbResponse.IsSuccessStatusCode || tmdbResponse.Content == null) return Result.Error();

        var externalPerson = MapToDomain(tmdbResponse.Content);

        if (!string.IsNullOrWhiteSpace(externalPerson.Biography) || lang == DefLang)
            return externalPerson;

        var fallbackResponse = await tmdbApi.GetPersonDetailsAsync(personId, DefLang, ct);
        if (fallbackResponse.Content == null) return externalPerson;

        var fallback = MapToDomain(fallbackResponse.Content);

        return externalPerson with
        {
            Biography = string.IsNullOrWhiteSpace(externalPerson.Biography) ? fallback.Biography : externalPerson.Biography,
        };
    }
    
    private static ExternalPerson MapToDomain(TmdbPersonDetailResponse tmdb)
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