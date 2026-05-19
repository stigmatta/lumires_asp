using JetBrains.Annotations;

namespace lumires.Core.Models;

[UsedImplicitly]
public record EmailSendCommand(string To, string Subject, string TemplateName, object Model);