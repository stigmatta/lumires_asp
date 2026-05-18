using Ardalis.Result;

namespace lumires.Domain.Exceptions;

public class ResultException(ResultStatus status, string message) : DomainException(message)
{
    public ResultStatus Status { get; } = status;
}