using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Core.Events.Films;

[UsedImplicitly]
public sealed class FilmReferencedEvent : IEvent
{
    public IReadOnlyCollection<int> ExternalIds { get; init; } = [];
    public string Language { get; init; } = null!;
}