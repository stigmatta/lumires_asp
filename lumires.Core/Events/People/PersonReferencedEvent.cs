using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Core.Events.People;

[UsedImplicitly]
public sealed class PersonReferencedEvent : IEvent
{
    public IReadOnlyCollection<(int, string)> IdsAndDepartments { get; init; } = [];
    public string Language { get; init; } = null!;
}