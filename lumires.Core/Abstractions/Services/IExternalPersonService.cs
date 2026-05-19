using Ardalis.Result;
using lumires.Core.Models;

namespace lumires.Core.Abstractions.Services;

public interface IExternalPersonService
{
    Task<Result<ExternalPerson>> GetPersonDetailsAsync(int personId, string lang, CancellationToken ct);
}