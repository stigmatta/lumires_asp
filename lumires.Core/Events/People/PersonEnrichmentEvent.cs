using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Core.Events.People;

[UsedImplicitly]
public sealed class PersonEnrichmentEvent : IEvent
{
    public IReadOnlyCollection<(int Id, string Department)> IdsAndDepartments { get; init; } = [];
    public string SkipLanguage { get; init; } = null!;
}