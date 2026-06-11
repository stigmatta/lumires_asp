using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Resources;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.FilmsLists.UpdateFilmsList;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db, IStringLocalizer<SharedResource> localizer) : IDataAccess
{
    internal async Task<Result> UpdateFilmsListAsync(Command command, Guid userId, CancellationToken ct)
    {
        var existingList = await db.FilmsLists.FirstOrDefaultAsync(l => l.Id == command.ListId, ct);

        if (existingList is null)
            return Result.NotFound();

        if (existingList.UserId != userId)
            return Result.Forbidden();

        if (command.FilmIds is { Count: > 0 })
        {
            var movieIds = await db.Films
                .Where(m => command.FilmIds.Contains(m.ExternalId))
                .Select(m => new { m.Id, m.ExternalId })
                .ToListAsync(ct);

            if (movieIds.Count != command.FilmIds.Count)
                return Result.Invalid(new ValidationError(localizer["ValidationError_SomeFilms_WereNotFound"]));
        }
        
        existingList.UpdateList(command.Title, command.Description, command.IsPrivate);

        db.FilmsLists.Update(existingList);
        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}