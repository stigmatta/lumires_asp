using System.Text.RegularExpressions;
using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed partial class User
{
    private User()
    {
    }

    public User(Guid id, string username, string email)
    {
        if (id == Guid.Empty) throw new UserValidationException("UserId is invalid");

        if (string.IsNullOrWhiteSpace(username))
            throw new UserValidationException("Username cannot be empty.");

        if (!IsUsernameValid(username))
            throw new UserValidationException("Username contains invalid characters or starts incorrectly.");

        if (!IsEmailValid(email))
            throw new UserValidationException("Email is not valid");

        Id = id;
        Username = username;
        Email = email;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string? Username { get; private set; }
    public string Email { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public ICollection<Collection> Collections { get; private set; } = new List<Collection>();


    public void SetAvatarUrl(string avatarUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(avatarUrl);
        AvatarUrl = avatarUrl;
    }
    
    public static bool IsUsernameValid(string username) => UsernameRegex().IsMatch(username);
    public static bool IsEmailValid(string email) => EmailRegex().IsMatch(email);

    [GeneratedRegex(@"^[a-zA-Z0-9][a-zA-Z0-9._]{2,19}$")]
    private static partial Regex UsernameRegex();

    [GeneratedRegex(@"^[\w\.\-]+@([\w\-]+\.)+[\w]{2,}$")]
    private static partial Regex EmailRegex();
}