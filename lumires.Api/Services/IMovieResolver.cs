
namespace lumires.Api.Services;

internal interface IMovieResolver
{
    Task<bool> EnsureMovieExistsAsync(int externalId, CancellationToken ct); //bool for check if it was already in db 
}