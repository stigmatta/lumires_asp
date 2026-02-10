using System.Globalization;
using System.Security.Claims;
using Core.Abstractions.Services;
using Core.Auth;

namespace Infrastructure.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public string UserRole => _user?.FindFirstValue("role") ?? UserRoles.User;
    public string UserTier => _user?.FindFirstValue("tier") ?? UserTiers.Free;

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
    public string CurrentLanguage => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    public string LangCulture => CurrentLanguage switch
    {
        "uk" => "uk-UA",
        _ => "en-US"
    };
}