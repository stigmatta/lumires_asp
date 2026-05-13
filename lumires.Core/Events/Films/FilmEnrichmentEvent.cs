using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Core.Events.Films;

[UsedImplicitly]
public sealed class FilmEnrichmentEvent : IEvent
{
    public IReadOnlyList<int> ExternalIds { get; init; } = [];
    public string SkipLanguage { get; init; } = null!;
}