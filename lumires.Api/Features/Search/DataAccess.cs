using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Search;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<SearchResponse> GetLists(string searchTerm, CancellationToken ct)
    {
        var lists = await db.FilmsLists
            .AsNoTracking()
            .Where(l =>  EF.Functions.Like(l.Title.ToLower(), $"%{searchTerm}%"))
            .Select(l => new ListResult(
                l.Id,
                l.Title,
                l.UserId,
                l.User.Username,
                l.Films.Count,
                l.LikesCount,
                l.Films
                    .Select(f => new FilmInListItem(f.Film.PosterPath))
                    .Take(4)
                    .ToList()
            ))
            .ToListAsync(ct);

        return new SearchResponse(Lists: lists);
    }
    
    internal async Task<SearchResponse> GetMembers(string searchTerm, CancellationToken ct)
    {
        var members = await db.Users
            .AsNoTracking()
            .Where(l =>  EF.Functions.Like(l.Username.ToLower(), $"%{searchTerm}%"))
            .Select(l => new MemberResult(
                l.Id,
                l.Username,
                l.AvatarUrl,
                0 // TODO when friends will be available
            ))
            .ToListAsync(ct);

        return new SearchResponse(Members: members);
    }

    internal async Task<SearchResponse> GetAll(string searchTerm, CancellationToken ct)
    {
        var lists = await db.FilmsLists
            .AsNoTracking()
            .Where(l =>  EF.Functions.Like(l.Title.ToLower(), $"%{searchTerm}%"))
            .Select(l => new ListResult(
                l.Id,
                l.Title,
                l.UserId,
                l.User.Username,
                l.Films.Count,
                l.LikesCount,
                l.Films
                    .Select(f => new FilmInListItem(f.Film.PosterPath))
                    .Take(4)
                    .ToList()
            ))
            .ToListAsync(ct);
        
        var members = await db.Users
            .AsNoTracking()
            .Where(l =>  EF.Functions.Like(l.Username.ToLower(), $"%{searchTerm}%"))
            .Select(l => new MemberResult(
                l.Id,
                l.Username,
                l.AvatarUrl,
                0 // TODO when friends will be available
            ))
            .ToListAsync(ct);
        
        return new SearchResponse(Lists: lists, Members: members);
    }
}