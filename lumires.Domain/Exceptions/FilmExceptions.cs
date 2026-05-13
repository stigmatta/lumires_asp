using Ardalis.Result;

namespace lumires.Domain.Exceptions;

public class FilmValidationException(string message, string field) : DomainException(message);

public class InvalidFilmOperationException(string message) : DomainException(message);

public class ExternalFilmException(ResultStatus status, string message) : DomainException(message)
{
    public ResultStatus Status { get; } = status;
}