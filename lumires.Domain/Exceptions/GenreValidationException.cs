namespace lumires.Domain.Exceptions;

public class GenreValidationException(string message, string field) : DomainException(message);