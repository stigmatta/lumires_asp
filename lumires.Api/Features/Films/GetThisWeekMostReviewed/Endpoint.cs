using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using ZiggyCreatures.Caching.Fusion;

namespace lumires.Api.Features.Films.GetThisWeekMostReviewed;

[UsedImplicitly]
internal sealed record WeeklyReviewedItem(
    int ExternalId,
    string Title,
    string? Quote,
    string Slug,
    string? BackdropPath,
    Guid ReviewerId,
    string ReviewerName,
    float? Rating);

[UsedImplicitly]
internal sealed record Response(IReadOnlyList<WeeklyReviewedItem> Items);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    IFusionCache cache,
    DataAccess db)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/films/most-reviewed/weekly");
        Description(x => x.WithTags("Films"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var cacheKey = CacheKeys.ThisWeekMostReviewedFilms(lang);

        Response = await cache.GetOrSetAsync<Response>(
            cacheKey,
            async (_, token) =>
            {
                var result = await db.GetThisWeekMostReviewed(lang, token);
                return result ?? new Response([]);
            },
            options => options.SetDuration(CacheDuration.Eternal)
                .SetFailSafe(true),
            ct
        );
    }
}