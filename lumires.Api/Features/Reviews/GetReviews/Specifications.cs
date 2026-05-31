using System.Linq.Expressions;
using LinqKit;
using lumires.Api.Enums.Common;
using lumires.Domain.Entities;

namespace lumires.Api.Features.Reviews.GetReviews;

internal static class Specifications
{
    private const int LongThreshold = 500;

    public static Expression<Func<Review, bool>> BuildFilter(Query req)
    {
        var filter = PredicateBuilder.New<Review>(true);

        var ratingFilter = BuildRating(req);
        filter = filter.And(ratingFilter);

        var contentFilter = BuildCategory(req);
        filter = filter.And(contentFilter);

        return filter;
    }

    private static Expression<Func<Review, bool>> BuildRating(Query req)
    {
        return req.Filter switch
        {
            RatingEnum.MoreThanFourHalf => r => r.Rating >= 4.5f && r.Rating <= 5f,
            RatingEnum.FourStars => r => r.Rating >= 4f && r.Rating < 4.5f,
            RatingEnum.ThreeStars => r => r.Rating >= 3f && r.Rating < 4f,
            RatingEnum.UnderThree => r => r.Rating < 3f,
            _ => r => true
        };
    }

    private static Expression<Func<Review, bool>> BuildCategory(Query req)
    {
        return req.Category switch // TODO with movie log
        {
            ContentFilterEnum.FirstWatches => r => r.Id != Guid.Empty, //TODO later
            ContentFilterEnum.LongForm => r => r.Text.Length >= LongThreshold,
            ContentFilterEnum.SpoilerFree => r => r.IsSpoilerFree == true,
            ContentFilterEnum.FromFriends => r => r.Id != Guid.Empty, //TODO later
            _ => r => true
        };
    }

    public static Func<IQueryable<Review>, IOrderedQueryable<Review>>? BuildSort(Query req)
    {
        return req.SortBy switch
        {
            ContentOrderEnum.MostLiked => q => q.OrderByDescending(r => r.LikesCount),
            ContentOrderEnum.MostReplies => q => q.OrderByDescending(r => r.ReviewComments.Count),
            ContentOrderEnum.MostRecent => q => q.OrderByDescending(r => r.CreatedAt),
            ContentOrderEnum.HighestRated => q => q.OrderByDescending(r => r.Rating),
            _ => null
        };
    }
}