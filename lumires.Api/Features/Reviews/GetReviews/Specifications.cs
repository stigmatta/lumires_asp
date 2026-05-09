using System.Linq.Expressions;
using lumires.Domain.Entities;

namespace lumires.Api.Features.Reviews.GetReviews;

internal static class Specifications
{
    public static Expression<Func<Review, bool>> BuildFilter(Query req)
    {
        return req.Filter switch
        {
            FilterEnum.FiveStars => r => r.Rating == 5m,
            FilterEnum.FourStars => r => r.Rating >= 4m && r.Rating < 5m,
            FilterEnum.ThreeStars => r => r.Rating >= 3m && r.Rating < 4m,
            FilterEnum.UnderThree => r => r.Rating < 3m,
            _ => r => true
        };
    }

    public static Func<IQueryable<Review>, IOrderedQueryable<Review>>? BuildSort(Query req)
    {
        return req.SortBy switch
        {
            SortEnum.MostLiked => q => q.OrderByDescending(r => r.LikesCount),
            SortEnum.MostReplies => q => q.OrderByDescending(r => r.ReviewComments.Count),
            SortEnum.MostRecent => q => q.OrderByDescending(r => r.CreatedAt),
            SortEnum.HighestRated => q => q.OrderByDescending(r => r.Rating),
            _ => null
        };
    }
}