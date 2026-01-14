using System.Security.Claims;
using lumires.Api.Shared.Abstractions;

namespace lumires.Api.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public Guid UserId =>
        Guid.TryParse(_user?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : Guid.Empty;

    public string? Email => _user?.FindFirstValue(ClaimTypes.Email);

    public bool IsEmailConfirmed =>
        bool.TryParse(
            _user?.FindFirstValue("email_verified"),
            out var confirmed
        ) && confirmed;

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated ?? false;
}