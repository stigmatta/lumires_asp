using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using lumires.Core.Helpers;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.GetUserFavouriteFilms;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;
    
    internal async Task<Result<Response>> GetFavouriteFilms(
        string username,
        string lang,
        CancellationToken ct)
    {
        var userExists = await db.Users
            .AnyAsync(u => u.Username == username , ct);

        if (!userExists)
            return Result.NotFound();

        var raw = await db.UserFavoriteFilms
            .Where(u => u.UserSettings.User.Username == username)
            .OrderBy(u => u.Order) 
            .Select(u => new
            {
                u.Film.ExternalId,
                Title = u.Film.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Title)
                    .First(),
                u.Film.PosterPath,
                ReleaseYear = u.Film.ReleaseDate.HasValue ? u.Film.ReleaseDate.Value.Year : (int?)null,
                Genres = u.Film.Genres
                    .Select(g =>
                        g.Localizations
                            .Where(gl => gl.LanguageCode == lang || gl.LanguageCode == DefLang)
                            .OrderByDescending(gl => gl.LanguageCode == lang)
                            .Select(gl => gl.Name)
                            .FirstOrDefault() ?? string.Empty
                    )
                    .ToArray(),
                u.Film.VoteAverage,
                u.Film.VoteCount,
                u.UserSettings.UserId,
                u.UserSettings.User.Username
            })
            .ToListAsync(ct);

        var ratings = raw.ToDictionary(
            f => f.ExternalId,
            f =>
            {
                var (rating, _) = CalculateFilmRating.Handle(
                    f.VoteAverage,
                    f.VoteCount,
                    f.VoteAverage, 
                    f.VoteCount);

                return rating;
            });

        var favouriteFilms = raw.Select(f => new FavouriteFilm(
            f.ExternalId,
            f.Title,
            f.PosterPath,
            f.ReleaseYear,
            f.Genres,
            ratings[f.ExternalId],
            f.UserId,
            f.Username
        )).ToList();

        return new Response(favouriteFilms);
    }

}