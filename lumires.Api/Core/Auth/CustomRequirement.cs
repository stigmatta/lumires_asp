using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace lumires.Api.Core.Auth;

[UsedImplicitly]
internal record CustomRequirement(string MinRole, string MinTier) : IAuthorizationRequirement;