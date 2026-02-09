using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace Contracts.Auth;

[UsedImplicitly]
public record CustomRequirement(string MinRole, string MinTier) : IAuthorizationRequirement;