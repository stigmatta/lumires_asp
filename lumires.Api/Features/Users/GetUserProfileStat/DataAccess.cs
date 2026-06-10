using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Users.GetUserProfileStat;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db, ICurrentUserService currentUserService) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<Response?> GetUserProfile(string username, string lang, CancellationToken ct)
    {
        var user = await db.Users
            .Where(x => x.Username == username)
            .Select(x => new { x.Id })
            .FirstOrDefaultAsync(ct);

        if (user is null) return null;

        var directors = await db.WatchedFilms
            .Where(w => w.UserId == user.Id)
            .SelectMany(w => w.Film.Directors)
            .GroupBy(d => d.Person.Localizations
                .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                .OrderByDescending(l => l.LanguageCode == lang)
                .Select(l => l.Name)
                .FirstOrDefault() ?? string.Empty)
            .OrderByDescending(g => g.Count())
            .Take(6)
            .Select(g => g.Key)
            .ToArrayAsync(ct);

        var decades = await db.WatchedFilms
            .Where(w => w.UserId == user.Id && w.Film.ReleaseDate.HasValue)
            .Select(w => w.Film.ReleaseDate!.Value.Year / 10 * 10)
            .GroupBy(decade => decade)
            .OrderByDescending(g => g.Count())
            .Take(6)
            .Select(g => g.Key + "s")
            .ToArrayAsync(ct);

        var genres = await db.WatchedFilms
            .Where(w => w.UserId == user.Id)
            .SelectMany(w => w.Film.Genres)
            .GroupBy(g => g.Localizations
                .Where(l => l.LanguageCode == lang)
                .Select(l => l.Name)
                .FirstOrDefault())
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
            .Where(g => g != null)
            .Select(g => g!)
            .ToArrayAsync(ct);

        var ratingsRaw = await db.UserFilmRatings
            .Where(r => r.UserId == user.Id)
            .GroupBy(r => r.Rating)
            .Select(g => new { Stars = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var ratings = ratingsRaw
            .OrderByDescending(s => s.Count)
            .ToDictionary(s => s.Stars, s => s.Count);

        return new Response(directors, decades, genres, ratings);
    }
}