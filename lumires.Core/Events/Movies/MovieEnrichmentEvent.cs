using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Core.Events.Movies;

[UsedImplicitly]
public sealed class MovieEnrichmentEvent : IEvent
{
    public int ExternalId { get; init; }
    public string SkipLanguage { get; init; } = null!;
}