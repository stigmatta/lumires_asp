using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class Collection
{
    public Guid Id { get; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public bool IsPrivate { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    private readonly List<CollectionMovie> _movies = [];
    public IReadOnlyCollection<CollectionMovie> Movies => _movies.AsReadOnly();

    private Collection() { }

    public Collection(string title, Guid userId, string? description = null, bool isPrivate = false)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new CollectionValidationException("Title cannot be empty");
            
        if (userId == Guid.Empty)
            throw new CollectionValidationException("UserId is required");

        Id = Guid.CreateVersion7(); 
        Title = title;
        UserId = userId;
        Description = description;
        IsPrivate = isPrivate;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddMovie(Guid movieId)
    {
        if (_movies.Any(m => m.MovieId == movieId))
            return; 

        var nextOrder = _movies.Count > 0 ? _movies.Max(m => m.Order) + 1 : 1;
        _movies.Add(new CollectionMovie(Id, movieId, nextOrder));
        
        UpdateTimestamp();
    }

    private void UpdateTimestamp() => UpdatedAt = DateTimeOffset.UtcNow;
}