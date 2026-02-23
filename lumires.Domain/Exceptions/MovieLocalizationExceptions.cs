namespace lumires.Domain.Exceptions;

public class MovieLocalizationValidationException(string message, string field) : DomainException(message);
