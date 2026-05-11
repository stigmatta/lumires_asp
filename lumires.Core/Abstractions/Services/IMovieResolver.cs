namespace lumires.Core.Abstractions.Services;

public interface IMovieResolver
{
    Task<bool>
        EnsureMovieExistsAsync(int externalId, string lang,
            CancellationToken ct); //bool for check if it was already in db 
}