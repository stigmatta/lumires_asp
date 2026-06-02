using lumires.Domain.Base;
using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class FilmsList : LikeableEntity<FilmsListLike>
{
    private readonly List<ListFilm> _films = [];

    private FilmsList()
    {
    }

    public FilmsList(string title, Guid userId, string? description = null, bool isPrivate = false)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title cannot be empty", nameof(title));

        if (userId == Guid.Empty)
            throw new DomainException("UserId is required", nameof(userId));

        Id = Guid.CreateVersion7();
        Title = title;
        UserId = userId;
        Description = description;
        IsPrivate = isPrivate;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public bool IsPrivate { get; private set; }
    public bool IsEditorPick { get; private set; }
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

    public void AddFilm(Film film)
    {
        if (_films.Any(m => m.FilmId == film.Id))
            return;
        
        var nextOrder = _films.Count > 0 ? _films.Max(m => m.Order) + 1 : 1;
        _films.Add(new ListFilm(Id, film, nextOrder));

        UpdateTimestamp();
    }

    public void SetUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        User = user;
        UserId = user.Id;
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

    public void SetEditorPick(bool editorPick)
    {
        IsEditorPick = editorPick;
    }
}