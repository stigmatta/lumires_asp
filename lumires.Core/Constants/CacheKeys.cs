namespace Core.Constants;

public static class CacheKeys
{
    public static string MovieKey(int id, string lang) => $"movie:{id}:{lang}";
    public static string MovieSources(int id, string region) => $"sources:{id}:{region}";

}