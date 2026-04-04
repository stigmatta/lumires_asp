using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class CollectionMovie
{
    public Guid CollectionId { get; private set; }
    public Guid MovieId { get; private set; }
    public int Order { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }

    public Collection Collection { get; private set; } = null!;
    public Movie Movie { get; private set; } = null!;

    private CollectionMovie() { }

    public CollectionMovie(Guid collectionId, Guid movieId, int order)
    {
        if (collectionId == Guid.Empty)
        {
            throw new CollectionValidationException("Collection ID is invalid");
        }

        if (movieId == Guid.Empty)
        {
            throw new CollectionValidationException("Movie ID is invalid");
        }

        if (order < 0)
        {
            throw new CollectionValidationException("Order cannot be negative");
        }
        
        CollectionId = collectionId;
        MovieId = movieId;
        Order = order;
        AddedAt = DateTimeOffset.UtcNow;
    }
}