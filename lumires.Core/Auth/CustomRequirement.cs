using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace Core.Auth;

[UsedImplicitly]
public record CustomRequirement(string MinRole, string MinTier) : IAuthorizationRequirement;