using lumires.Domain.Entities;

namespace lumires.Core.Abstractions.Services;

public interface IPersonResolver
{
    Task<Dictionary<int, Person>> ResolveAsync(
        IEnumerable<(int ExternalId, string Name, string department)> persons,
        string languageCode,
        CancellationToken ct = default);

    Task<bool> EnsurePersonExistsAsync((int externalId, string Deparment) idAndDep,
        string language,
        CancellationToken ct);
}