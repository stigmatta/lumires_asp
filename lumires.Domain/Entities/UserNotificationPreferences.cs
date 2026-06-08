namespace lumires.Domain.Entities;

public class UserNotificationPreferences
{
    private UserNotificationPreferences()
    {
    }

    public UserNotificationPreferences(
        bool newFollower,
        bool likes,
        bool replies,
        bool activity,
        bool saves,
        bool digest)
    {
        NewFollower = newFollower;
        LikesOnContent = likes;
        RepliesAndMentions = replies;
        ActivityFromFollowed = activity;
        SavesOnLists = saves;
        WeeklyDigest = digest;
    }

    public bool NewFollower { get; private set; }
    public bool LikesOnContent { get; private set; }
    public bool RepliesAndMentions { get; private set; }
    public bool ActivityFromFollowed { get; private set; }
    public bool SavesOnLists { get; private set; }
    public bool WeeklyDigest { get; private set; }
}