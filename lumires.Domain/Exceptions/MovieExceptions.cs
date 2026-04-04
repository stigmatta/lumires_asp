using Ardalis.Result;

namespace lumires.Domain.Exceptions;

public class MovieValidationException(string message, string field) : DomainException(message);

public class InvalidMovieOperationException(string message) : DomainException(message);

public class ExternalMovieException(ResultStatus status, string message) : DomainException(message)
{
    public ResultStatus Status { get; } = status;
}