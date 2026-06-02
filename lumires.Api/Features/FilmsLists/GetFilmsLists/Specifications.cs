using System.Linq.Expressions;
using LinqKit;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.GetFilmsLists;

internal static class Specifications
{
    public static Expression<Func<FilmsList, bool>> BuildFilter(Query req)
    {
        var filter = PredicateBuilder.New<FilmsList>(true);

        var contentFilter = BuildCategory(req);
        filter = filter.And(contentFilter);

        if (string.IsNullOrWhiteSpace(req.SearchTerm)) return filter;

        var search = req.SearchTerm.Trim().ToLowerInvariant(); 

        filter = filter.And(fl =>
            fl.Films.Any(f =>
                f.Film.Localizations.Any(l =>
                    EF.Functions.Like(l.Title.ToLower(), $"%{search}%")
                )
            )
        );

        return filter;
    }

    private static Expression<Func<FilmsList, bool>> BuildCategory(Query req)
    {
        return req.Category switch // TODO with movie log
        {
            ContentFilterEnum.EditorPicks => fl => fl.IsEditorPick,
            ContentFilterEnum.NewLists => fl => fl.CreatedAt >= DateTime.UtcNow.AddDays(-3),
            ContentFilterEnum.RecentlyUpdated => fl => fl.UpdatedAt >= DateTime.UtcNow.AddDays(-3),
            ContentFilterEnum.Trending => fl => fl.LikesCount > 3,
            ContentFilterEnum.FriendsLists => fl => fl.Id != Guid.Empty, //TODO friends 
            _ => r => true
        };
    }

    public static Func<IQueryable<FilmsList>, IOrderedQueryable<FilmsList>>? BuildSort(Query req)
    {
        return req.SortBy switch
        {
            ListContentOrderEnum.MostFilms => q => q.OrderByDescending(fl => fl.Films.Count),
            ListContentOrderEnum.MostPopular => q => q.OrderByDescending(fl => fl.LikesCount),
            ListContentOrderEnum.MostRecent => q => q.OrderByDescending(fl => fl.CreatedAt),
            _ => null
        };
    }
}