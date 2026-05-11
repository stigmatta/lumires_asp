using lumires.Domain.Entities;

namespace lumires.Core.Abstractions.Services;

public interface IPersonResolver
{
    Task<Dictionary<int, Person>> ResolveAsync(
        IEnumerable<(int ExternalId, string Name)> persons,
        CancellationToken ct);
}