namespace lumires.Core.Constants;

public static class CacheKeys
{
    public static string FilmKey(int id, string lang)
    {
        return $"film:{id}:{lang}";
    }

    public static string FilmSources(int id, string region)
    {
        return $"sources:{id}:{region}";
    }

    public static string FilmSourceExternalId(int id)
    {
        return $"wm_id:{id}";
    }

    public static string ThisWeekPopularFilms(string lang)
    {
        return $"this_week_popular_films:{lang}";
    }

    public static string ThisWeekRecentFilms(string lang)
    {
        return $"this_week_recent_films:{lang}";
    }

    public static string ThisWeekMostReviewedFilms(string lang)
    {
        return $"this_week_most_reviewed_films:{lang}";
        
    }

    public static string GenresList(string lang)
    {
        return $"genres:{lang}";
    }

    public static string FilmsSummary()
    {
        return "films_summary";
    }

    public static string ReviewsSummary()
    {
        return "reviews_summary";
    }
}