using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.UpdateReview;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> UpdateReviewAsync(Command command, Guid userId, CancellationToken ct)
    {
        var movieId = await db.Films
            .Where(m => m.ExternalId == command.FilmId)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(ct);

        if (movieId == Guid.Empty) return Result.NotFound();

        var existingReview = await db.Reviews.FirstOrDefaultAsync(x => x.Id == command.ReviewId, ct);
        
        if (existingReview is null)
            return Result.NotFound();
        
        if (existingReview.UserId != userId)
            return Result.Forbidden();

        existingReview.UpdateReview(command.Title, command.Text, command.Rating, command.IsSpoilerFree);
        db.Reviews.Update(existingReview);
        
        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}