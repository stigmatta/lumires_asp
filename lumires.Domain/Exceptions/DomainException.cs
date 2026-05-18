namespace lumires.Domain.Exceptions;

public class DomainException(string message, string? field = null) : Exception(message);