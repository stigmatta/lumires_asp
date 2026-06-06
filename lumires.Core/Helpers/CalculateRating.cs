namespace lumires.Core.Helpers;

public static class CalculateFilmRating
{
    public static (float Rating, int TotalVotes) Handle(
        float externalRating,
        int externalVoteCount,
        float internalRating,
        int internalVoteCount)
    {
        var totalVotes = externalVoteCount + internalVoteCount;

        if (totalVotes == 0)
            return (0, 0);

        var weightedSum =
            externalRating * externalVoteCount +
            internalRating * internalVoteCount;

        return (weightedSum / totalVotes, totalVotes);
    }
}