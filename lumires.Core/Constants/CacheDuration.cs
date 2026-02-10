namespace Core.Constants;

public static class CacheDuration
{
    public static readonly TimeSpan Short = TimeSpan.FromSeconds(30);

    public static readonly TimeSpan Medium = TimeSpan.FromHours(1);

    public static readonly TimeSpan Long = TimeSpan.FromHours(6);

    public static readonly TimeSpan Eternal = TimeSpan.FromDays(2);
}