using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public class SavedList
{
    private readonly List<User> _users = [];

    private SavedList()
    {
    }

    public SavedList(Guid userId, Guid listId)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is invalid", nameof(userId));

        if (listId == Guid.Empty) throw new DomainException("List id is invalid");
        Id = Guid.CreateVersion7();
        UserId = userId;
        ListId = listId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Guid ListId { get; private set; }
    public FilmsList List { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; }

    public IReadOnlyCollection<User> Users =>
        _users.AsReadOnly();
}