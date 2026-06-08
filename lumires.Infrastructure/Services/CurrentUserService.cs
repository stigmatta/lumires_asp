using System.Globalization;
using System.Security.Claims;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Auth;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor, IAppDbContext db) : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public string UserRole => _user?.FindFirstValue("role") ?? UserRoles.User;

    public string UserTier => _user?.FindFirstValue("tier") ?? UserTiers.Free;

    public Guid UserId =>
        Guid.TryParse(_user?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : Guid.Empty;

    //Email always should be existing in the claims
    public string Email => _user?.FindFirstValue(ClaimTypes.Email)!;

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

    public async Task<string> GetUsernameAsync(CancellationToken ct)
    {
        var result = await db.Users
            .Where(u => u.Id == UserId)
            .Select(u => new
            {
                u.Username, u.Email
            })
            .FirstOrDefaultAsync(ct);

        if (result == null)
            return string.Empty;

        return !string.IsNullOrWhiteSpace(result.Username)
            ? result.Username
            : result.Email.Split('@')[0];
    }
}