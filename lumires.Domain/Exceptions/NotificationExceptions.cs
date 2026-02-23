namespace lumires.Domain.Exceptions;

public class InvalidNotificationOperationException(string message) : DomainException(message);

public class NotificationValidationException(string message) : DomainException(message);