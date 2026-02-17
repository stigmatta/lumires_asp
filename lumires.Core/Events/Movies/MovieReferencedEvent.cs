using FastEndpoints;
using JetBrains.Annotations;

namespace Core.Events.Movies;

[UsedImplicitly]
public sealed class MovieReferencedEvent : IEvent
{
    public int ExternalId { get; init; }
}