using lumires.Domain.Base;
using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class FilmsList: LikeableEntity<FilmsListLike>
{
    private readonly List<ListFilm> _films = [];

    private FilmsList()
    {
    }

    public FilmsList(string title, Guid userId, string? description = null, bool isPrivate = false)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new FilmsListValidationException("Title cannot be empty");

        if (userId == Guid.Empty)
            throw new FilmsListValidationException("UserId is required");

        Id = Guid.CreateVersion7();
        Title = title;
        UserId = userId;
        Description = description;
        IsPrivate = isPrivate;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public bool IsPrivate { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public IReadOnlyCollection<ListFilm> Films => _films.AsReadOnly();

    public void AddFilm(Guid filmId)
    {
        if (_films.Any(m => m.FilmId == filmId))
            return;

        var nextOrder = _films.Count > 0 ? _films.Max(m => m.Order) + 1 : 1;
        _films.Add(new ListFilm(Id, filmId, nextOrder));

        UpdateTimestamp();
    }

    private void UpdateTimestamp()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    protected override Guid GetUserId(FilmsListLike like)
    {
        return like.UserId;
    }

    protected override FilmsListLike CreateLike(Guid userId)
    {
        return new FilmsListLike { FilmsListId = Id, UserId = userId };
    }
}