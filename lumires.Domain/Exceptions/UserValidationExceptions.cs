namespace lumires.Domain.Exceptions;

public class UserValidationException(string message) : DomainException(message);