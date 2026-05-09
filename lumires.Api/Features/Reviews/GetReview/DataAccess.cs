using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.GetReview;


[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Response?> GetReviewByIdAsync(Query query, CancellationToken ct)
    {
        return await db.Reviews
            .Where(x => x.Id == query.ReviewId)
            .Select(x => new Response(x.Id))
            .FirstOrDefaultAsync(ct);  
    }
}