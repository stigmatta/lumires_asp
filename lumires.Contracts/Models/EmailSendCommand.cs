namespace Contracts.Models;

public record EmailSendCommand(string To, string Subject, string TemplateName, object Model);