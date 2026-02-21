namespace Core.Constants;

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
}