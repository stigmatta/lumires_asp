using Ardalis.Result;
using lumires.Domain.Exceptions;

namespace Infrastructure.Exceptions;

public class WatchmodeException(ResultStatus status, string message) : DomainException(message)
{
    public ResultStatus Status { get; } = status;
}
