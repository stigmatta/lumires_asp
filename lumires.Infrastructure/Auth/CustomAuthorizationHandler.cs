using lumires.Core.Auth;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Auth;

internal sealed class CustomAuthorizationHandler
    : AuthorizationHandler<CustomRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CustomRequirement requirement)
    {
        var role = context.User.FindFirst("role")?.Value;
        var tier = context.User.FindFirst("tier")?.Value;

        if (role == UserRoles.Admin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var roleSatisfied = requirement.MinRole switch
        {
            UserRoles.User => true,
            UserRoles.Moderator => role == UserRoles.Moderator,
            _ => false
        };

        var tierSatisfied = requirement.MinTier switch
        {
            UserTiers.Free => true,
            UserTiers.Pro => tier is UserTiers.Pro or UserTiers.Patron,
            UserTiers.Patron => tier == UserTiers.Patron,
            _ => false
        };

        if (roleSatisfied && tierSatisfied)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}