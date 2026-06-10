using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Core.Mappers;
using lumires.Domain.Enums;

namespace lumires.Api.Features.FilmPeople.GetSimilarDirectors;

[UsedImplicitly]
internal sealed record Query(int Id);

[UsedImplicitly]
internal sealed record DirectorItem(int DirectorId, string? ProfilePath, string Name);

[UsedImplicitly]
internal sealed record Response(
    IReadOnlyCollection<DirectorItem> SimilarDirectors);

internal sealed class Endpoint(
    IPersonResolver personResolver,
    ICurrentUserService currentUserService,
    DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/directors/{Id:int}/similar");
        Description(x => x.WithTags("People"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;

        var idAndDep = (query.Id, nameof(PersonDepartment.Directing));

        await personResolver.EnsurePersonExistsAsync(idAndDep, lang, ct);

        var response = await db.GetSimilarDirectors(query.Id, lang, ct);

        // Similar directors are discovered from synced film credits, where persons
        // are created with only a localized name and no PersonDetail row — so their
        // ProfilePath is null until their own page is opened. Enrich any results that
        // are still missing a profile path so the client gets avatars here too.
        var missingProfile = response.SimilarDirectors
            .Where(d => string.IsNullOrEmpty(d.ProfilePath))
            .Select(d => d.DirectorId)
            .ToList();

        if (missingProfile.Count > 0)
        {
            foreach (var id in missingProfile)
                await personResolver.EnsurePersonExistsAsync(
                    (id, nameof(PersonDepartment.Directing)), lang, ct);

            response = await db.GetSimilarDirectors(query.Id, lang, ct);
        }

        await Send.OkAsync(response, ct);
    }
}