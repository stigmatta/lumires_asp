using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;

namespace lumires.Api.Features.Collections.Commands.CreateCollection;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Guid> CreateCollectionAsync(Command command, Guid userId, CancellationToken ct)
    {
        var collection = new Collection(
            command.Title,
            userId,
            command.Description,
            command.IsPrivate
        );

        if (command.MovieIds is { Count: > 0 })
            foreach (var movieId in command.MovieIds)
                collection.AddMovie(movieId);

        db.Collections.Add(collection);
        await db.SaveChangesAsync(ct);

        return collection.Id;
    }
}