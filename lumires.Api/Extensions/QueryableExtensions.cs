using System.Linq.Expressions;

namespace lumires.Api.Extensions;

internal static class QueryableExtensions
{
    public static IQueryable<T> ApplyFilter<T>(
        this IQueryable<T> query,
        Expression<Func<T, bool>>? filter)
    {
        return filter == null ? query : query.Where(filter);
    }

    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        Func<IQueryable<T>, IOrderedQueryable<T>>? sort)
    {
        return sort == null ? query : sort(query);
    }

    public static IQueryable<T> ApplyPaging<T>(
        this IQueryable<T> query,
        int? page,
        int? pageSize)
    {
        if (page == null || pageSize == null)
            return query;

        return query
            .Skip((page.Value - 1) * pageSize.Value)
            .Take(pageSize.Value);
    }
}