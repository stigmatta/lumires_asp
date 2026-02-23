namespace lumires.Domain.Exceptions;

public class MovieValidationException(string message, string field) : DomainException(message);

public class InvalidMovieOperationException(string message) : DomainException(message);
