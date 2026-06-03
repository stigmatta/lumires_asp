using lumires.Domain.Enums;
using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class UsersRelationship
{
    private UsersRelationship()
    {
    }

    public UsersRelationship(Guid sourceUserId, Guid targetUserId, UserRelationshipType type,
        UserRelationshipStatus status)
    {
        if (sourceUserId == Guid.Empty) throw new DomainException("SourceUserId is invalid", nameof(sourceUserId));

        if (targetUserId == Guid.Empty) throw new DomainException("TargetUserId is invalid", nameof(sourceUserId));

        SourceUserId = sourceUserId;
        TargetUserId = targetUserId;
        Id = Guid.CreateVersion7();
        Type = type;
        Status = status;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid SourceUserId { get; private set; }
    public User SourceUser { get; private set; } = null!;

    public Guid TargetUserId { get; private set; }
    public User TargetUser { get; private set; } = null!;

    public UserRelationshipType Type { get; private set; }
    public UserRelationshipStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public void SetStatus(UserRelationshipStatus status)
    {
        Status = status;
    } 
    
    public void SetType(UserRelationshipType type)
    {
        Type = type;
    } 
}