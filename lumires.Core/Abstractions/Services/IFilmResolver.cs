namespace lumires.Core.Abstractions.Services;

public interface IFilmResolver
{
    Task<bool>
        EnsureFilmExistsAsync(int externalId, string lang,
            CancellationToken ct); //bool for check if it was already in db 
}