using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.CreateReview;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result<Guid>> CreateReviewAsync(Command command, Guid userId, CancellationToken ct)
    {
        var movieId = await db.Films
            .Where(m => m.ExternalId == command.FilmId)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(ct);

        if (movieId == Guid.Empty) return Result.NotFound();

        var review = new Review(userId, movieId, command.Title, command.Text, command.Rating,
            command.IsSpoilerFree); //TODO When MovieLogs table will be created - change isFirstWatch

        await db.Reviews.AddAsync(review, ct);
        await db.SaveChangesAsync(ct);

        return review.Id;
    }
}