namespace lumires.Api.Infrastructure.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string? Email { get; }
    bool IsEmailConfirmed { get; }
    bool IsAuthenticated { get; }
}