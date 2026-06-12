using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.GetFilmsList;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    public async Task<Result<Response?>> GetFilmsListAsync(Guid id, string lang, Guid userId, CancellationToken ct)
    {
        var list = await db.FilmsLists
            .Where(fl => fl.Id == id)
            .Select(fl => new
            {
                fl.Id,
                fl.Title,
                fl.UserId,
                fl.User.Username,
                fl.IsPrivate,
                LastActivity = fl.UpdatedAt ?? fl.CreatedAt,
                IsLikedByMe = fl.Likes.Any(l => l.UserId == userId),
                IsSavedByMe = fl.SavedLists.Any(l => l.UserId == userId),
                Films = fl.Films
                    .OrderBy(m => m.Order)
                    .Select(m => new ListFilmItem(
                        m.Film.ExternalId,
                        m.Film.Localizations
                            .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                            .OrderByDescending(l => l.LanguageCode == lang)
                            .Select(l => l.Title)
                            .FirstOrDefault() ?? string.Empty,
                        m.Film.PosterPath,
                        m.Order
                    ))
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);


        if (list is null) return Result.NotFound();

        if (list.IsPrivate && list.UserId != userId)
            return Result.Forbidden();

        return new Response(
            list.Id,
            list.Title,
            list.UserId,
            list.Username,
            list.LastActivity,
            list.IsLikedByMe,
            list.IsSavedByMe,
            list.Films
        );
    }
}