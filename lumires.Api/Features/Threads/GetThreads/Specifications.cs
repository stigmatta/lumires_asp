using System.Linq.Expressions;
using LinqKit;
using lumires.Api.Enums.Common;
using lumires.Domain.Entities;

namespace lumires.Api.Features.Threads.GetThreads;

internal static class Specifications
{
    private const int LongThreshold = 500;

    public static Expression<Func<UserThread, bool>> BuildFilter(Query req, IEnumerable<Guid>? friendIds = null)
    {
        var filter = PredicateBuilder.New<UserThread>(true);

        var contentFilter = BuildCategory(req, friendIds);
        filter = filter.And(contentFilter);
        
        if (req.UserId.HasValue)
            filter = filter.And(fl =>
                fl.UserId == req.UserId.Value);

        return filter;
    }

    private static Expression<Func<UserThread, bool>> BuildCategory(Query req, IEnumerable<Guid>? friendIds = null)
    {
        return req.Category switch
        {
            ContentFilterEnum.LongForm => t => t.Text.Length >= LongThreshold,
            ContentFilterEnum.FromFriends => r => friendIds != null && friendIds.Contains(r.UserId),
            ContentFilterEnum.SpoilerFree => t => t.IsSpoilerFree == true,
            _ => r => true
        };
    }

    public static Func<IQueryable<UserThread>, IOrderedQueryable<UserThread>>? BuildSort(Query req)
    {
        return req.SortBy switch
        {
            ContentOrderEnum.MostLiked => q => q.OrderByDescending(r => r.LikesCount),
            ContentOrderEnum.MostReplies => q => q.OrderByDescending(r => r.UserThreadComments.Count),
            ContentOrderEnum.MostRecent => q => q.OrderByDescending(r => r.CreatedAt),
            _ => null
        };
    }
}