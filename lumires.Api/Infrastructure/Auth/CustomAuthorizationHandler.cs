using lumires.Api.Core.Auth;
using Microsoft.AspNetCore.Authorization;

namespace lumires.Api.Infrastructure.Auth;

internal class CustomAuthorizationHandler
    : AuthorizationHandler<CustomRequirement>
{
    
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CustomRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
            
        var userRole = context.User.FindFirst("role")?.Value ?? UserRoles.User;
        var userTier = context.User.FindFirst("tier")?.Value ?? UserTiers.Free;
        
        if (userRole == UserRoles.Admin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        ArgumentNullException.ThrowIfNull(requirement);
        
        var roleSatisfied = requirement.MinRole switch
        {
            UserRoles.User => true,
            UserRoles.Moderator => userRole == UserRoles.Moderator,
            _ => false
        };

        var tierSatisfied = requirement.MinTier switch
        {
            UserTiers.Free => true,
            UserTiers.Pro => userTier is UserTiers.Pro or UserTiers.Patron,
            UserTiers.Patron => userTier == UserTiers.Patron,
            _ => false
        };

        if (roleSatisfied && tierSatisfied) context.Succeed(requirement);

        return Task.CompletedTask;
    }
}