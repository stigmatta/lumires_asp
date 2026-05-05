namespace lumires.Domain.Exceptions;

public class GenreLocalizationValidationException(string message, string field) : DomainException(message);
