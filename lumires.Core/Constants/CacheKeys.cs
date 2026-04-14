namespace lumires.Core.Constants;

public static class CacheKeys
{
    public static string MovieKey(int id, string lang)
    {
        return $"movie:{id}:{lang}";
    }

    public static string MovieSources(int id, string region)
    {
        return $"sources:{id}:{region}";
    }

    public static string MovieSourceExternalId(int id)
    {
        return $"wm_id:{id}";
    }

    public static string ThisWeekPopularMovies(string lang)
    {
        return $"this_week_popular_movies:{lang}";
    }

    public static string ThisWeekRecentMovies(string lang)
    {
        return $"this_week_recent_movies:{lang}";
    }
}