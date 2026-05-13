namespace lumires.Domain.Exceptions;

public class FilmLocalizationValidationException(string message, string field) : DomainException(message);