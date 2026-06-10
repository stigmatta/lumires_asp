using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Enums;

namespace lumires.Api.Features.FilmPeople.GetSimilarActors;

[UsedImplicitly]
internal sealed record Query(int Id);

[UsedImplicitly]
internal sealed record ActorItem(int ActorId, string? ProfilePath, string Name);

[UsedImplicitly]
internal sealed record Response(
    IReadOnlyCollection<ActorItem> SimilarActors);

internal sealed class Endpoint(
    IPersonResolver personResolver,
    ICurrentUserService currentUserService,
    DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/actors/{Id:int}/similar");
        Description(x => x.WithTags("People"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;

        var idAndDep = (query.Id, nameof(PersonDepartment.Acting));

        await personResolver.EnsurePersonExistsAsync(idAndDep, lang, ct);

        var response = await db.GetSimilarActors(query.Id, lang, ct);

        // Similar actors are discovered from synced film credits, where persons
        // are created with only a localized name and no PersonDetail row — so their
        // ProfilePath is null until their own page is opened. Enrich any results that
        // are still missing a profile path so the client gets avatars here too.
        var missingProfile = response.SimilarActors
            .Where(a => string.IsNullOrEmpty(a.ProfilePath))
            .Select(a => a.ActorId)
            .ToList();

        if (missingProfile.Count > 0)
        {
            foreach (var id in missingProfile)
                await personResolver.EnsurePersonExistsAsync(
                    (id, nameof(PersonDepartment.Acting)), lang, ct);

            response = await db.GetSimilarActors(query.Id, lang, ct);
        }

        await Send.OkAsync(response, ct);
    }
}