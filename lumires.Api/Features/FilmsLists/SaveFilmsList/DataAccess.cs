using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.SaveFilmsList;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result<Guid>> SaveListAsync(Command command, Guid userId, CancellationToken ct)
    {
        var existingList = await db.FilmsLists.FirstOrDefaultAsync(f => f.Id == command.ListId, ct);

        if (existingList is null) return Result.NotFound();
        
        if (existingList.IsPrivate) return Result.Forbidden();

        var alreadySaved =
            await db.SavedLists.AnyAsync(f => f.ListId == command.ListId && f.UserId == userId, ct);

        if (alreadySaved) return Result.NoContent();

        var newlySaved = new SavedList(userId, existingList.Id);
        db.SavedLists.Add(newlySaved);

        await db.SaveChangesAsync(ct);

        return newlySaved.Id; 
    }
}