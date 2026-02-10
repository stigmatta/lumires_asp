namespace Core.Auth;

public static class CustomPolicies
{
    public const string AdminOnly = nameof(AdminOnly);
    public const string StaffOnly = nameof(StaffOnly);
    public const string PatronOnly = nameof(PatronOnly);
    public const string TierOnly = nameof(TierOnly);
}