namespace lumires.Api.Core.Abstractions;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string? Email { get; }
    bool IsEmailConfirmed { get; }
    bool IsAuthenticated { get; }
    string CurrentLanguage { get; }
    string LangCulture { get; }
}