using System.Linq.Expressions;
using LinqKit;
using lumires.Domain.Entities;

namespace lumires.Api.Features.FilmsLists.GetUserLikedFilmsLists;

internal static class Specifications
{
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