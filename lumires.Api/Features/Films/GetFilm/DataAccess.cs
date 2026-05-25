using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using lumires.Core.Helpers;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.GetFilm;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<Response?> GetFilmByIdAsync(int tmdbId, string lang, CancellationToken ct)
{
    var raw = await db.Films
        .AsNoTracking()
        .Where(m => m.ExternalId == tmdbId)
        .Select(m => new
        {
            m.ExternalId,
            m.ReleaseDate,
            m.TrailerUrl,
            m.PosterPath,
            m.BackdropPath,
            Localization = m.Localizations
                .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                .OrderByDescending(l => l.LanguageCode == lang)
                .Select(l => new LocalizationResponse(
                    l.LanguageCode,
                    l.Title,
                    l.Description,
                    l.Tagline))
                .FirstOrDefault(),
            Genres = m.Genres
                .Select(g => new GenreItemResponse(
                    g.ExternalId,
                    g.Localizations
                        .Where(gl => gl.LanguageCode == lang || gl.LanguageCode == DefLang)
                        .OrderByDescending(gl => gl.LanguageCode == lang)
                        .Select(gl => gl.Name)
                        .FirstOrDefault() ?? string.Empty,
                    lang))
                .ToList(),
            Cast = m.Cast
                .Select(c => new
                {
                    c.Person.ExternalId,
                    Localizations = c.Person.Localizations.ToList()
                })
                .ToList(),
            Directors = m.Directors
                .Select(c => new
                {
                    c.Person.ExternalId,
                    Localizations = c.Person.Localizations.ToList()
                })
                .ToList(),
            m.ProductionCompany,
            m.Runtime,
            m.VoteAverage,
            m.VoteCount,
            Ratings = m.UserRatings
                .Select(ur => ur.Rating),
            RatingCount = m.UserRatings.Count
        })
        .SingleOrDefaultAsync(ct);

    if (raw is null) return null;

    var (rating, totalVotes) = CalculateFilmRating.Handle(
        raw.VoteAverage,
        raw.VoteCount,
        raw.Ratings.Any() ? raw.Ratings.Average() : 0f,
        raw.RatingCount);

    return new Response(
        raw.ExternalId,
        raw.ReleaseDate,
        raw.TrailerUrl,
        raw.PosterPath,
        raw.BackdropPath,
        raw.Localization,
        new GenresResponse(raw.Genres),
        [.. raw.Cast
            .Select(c => new PersonShortItem(
                GetPersonNameWithFallback(c.Localizations, lang),
                c.ExternalId))],
        [.. raw.Directors
            .Select(c => new PersonShortItem(
                GetPersonNameWithFallback(c.Localizations, lang),
                c.ExternalId))],
        raw.ProductionCompany,
        raw.Runtime,
        rating,
        totalVotes
    );
}

    private static string GetPersonNameWithFallback(
        List<PersonLocalization> localizations,
        string requestedLang)
    {
        if (localizations.Count == 0)
            return string.Empty;

        var exact = localizations.FirstOrDefault(l => l.LanguageCode == requestedLang);
        if (exact != null)
            return exact.Name;

        var fallback = localizations.FirstOrDefault(l => l.LanguageCode == DefLang);
        return fallback != null ? fallback.Name : localizations.First().Name;
    }
}