using System.Globalization;
using System.Linq.Expressions;
using LinqKit;
using lumires.Api.Enums.Common;
using lumires.Core.Constants;
using lumires.Domain.Entities;

namespace lumires.Api.Features.Users.GetUserLikedFilms;

internal enum FilmContentOrder
{
    MostRecent,
    MostLiked,
    MostReplies,
    HighestRated,
    LeastRated
}
internal static class Specifications
{
    public static Expression<Func<Film, bool>> BuildFilter(Query req, string lang)
    {
        var filter = PredicateBuilder.New<Film>(true);

        filter = filter.And(BuildRatingFilter(req));

        if (!(req.Genres?.Length > 0)) return filter;

        var selectedGenres = req.Genres
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Select(g =>
                char.ToUpper(g[0], CultureInfo.InvariantCulture) + g[1..].ToLower(CultureInfo.InvariantCulture))
            .ToList();

        filter = filter.And(f =>
            selectedGenres.All(selected =>
                f.Genres.Any(g =>
                    g.Localizations.Any(gl =>
                        (gl.LanguageCode == lang || gl.LanguageCode == LocalizationConstants.DefaultCulture)
                        && gl.Name == selected))));


        return filter;
    }

    public static Func<IQueryable<Film>, IOrderedQueryable<Film>>? BuildSort(Query req)
    {
        return req.SortBy switch
        {
            FilmContentOrder.MostLiked => q => q.OrderByDescending(r => r.LikesCount), 
            FilmContentOrder.MostReplies => q => q.OrderByDescending(r => r.Reviews.Count),
            FilmContentOrder.MostRecent => q => q.OrderByDescending(r => r.ReleaseDate),
            FilmContentOrder.HighestRated => q => q.OrderByDescending(r => r.VoteAverage),
            FilmContentOrder.LeastRated => q => q.OrderBy(r => r.VoteAverage),
            _ => null
        };
    }

    private static Expression<Func<Film, bool>> BuildRatingFilter(Query req)
    {
        return req.Rating switch
        {
            RatingEnum.MoreThanFourHalf => f => f.VoteAverage >= 4.5 && f.VoteAverage < 5,
            RatingEnum.FourStars => f => f.VoteAverage >= 4 && f.VoteAverage < 4.5,
            RatingEnum.ThreeStars => f => f.VoteAverage >= 3 && f.VoteAverage < 4,
            RatingEnum.UnderThree => f => f.VoteAverage < 3,
            _ => f => true
        };
    }
}