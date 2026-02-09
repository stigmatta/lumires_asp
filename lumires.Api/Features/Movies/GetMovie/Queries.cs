using Contracts.Constants;
using JetBrains.Annotations;
using lumires.Api.Core.Abstractions;
using lumires.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Movies.GetMovie;

[UsedImplicitly]
internal class Queries(AppDbContext db) : IQuery
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    private static readonly Func<AppDbContext, int, string, CancellationToken, Task<Response?>>
        GetLocalizedMovieCompiledAsync =
            EF.CompileAsyncQuery((AppDbContext db, int tmdbId, string lang, CancellationToken ct) =>
                db.Movies
                    .AsNoTracking()
                    .Where(m => m.TmdbId == tmdbId)
                    .Select(m => new Response(
                        m.TmdbId,
                        m.Year,
                        m.Localizations
                            .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                            .OrderByDescending(l => l.LanguageCode == lang) 
                            .Select(l => new LocalizationResponse(
                                l.LanguageCode,
                                l.Title,
                                l.Description
                            ))
                            .FirstOrDefault() 
                    ))
                    .SingleOrDefault()
            );

    internal Task<Response?> GetMovieByIdAsync(int tmdbId, string lang, CancellationToken ct)
    {
        return GetLocalizedMovieCompiledAsync(db, tmdbId, lang, ct);
    }
}