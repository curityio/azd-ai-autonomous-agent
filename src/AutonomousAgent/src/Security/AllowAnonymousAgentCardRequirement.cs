

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

/*
 * Enable anonymous authentication for the agent card endpoint
 */
public class AllowAnonymousAgentCardRequirement: AuthorizationHandler<AllowAnonymousAgentCardRequirement>, IAuthorizationRequirement
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AllowAnonymousAgentCardRequirement requirement)
    {
        var httpContext = context.Resource as DefaultHttpContext;
        if (httpContext != null)
        {
            if (httpContext.Request.Path.Value?.ToLowerInvariant() == "/.well-known/agent-card.json")
            {
                context.Succeed(requirement);

                var denyAnonymousUserRequirements = context.PendingRequirements.OfType<DenyAnonymousAuthorizationRequirement>();
                foreach (var denyAnonymousUserRequirement in denyAnonymousUserRequirements)
                {
                    context.Succeed(denyAnonymousUserRequirement);
                }
            }
        }

        return Task.CompletedTask;
    }
}
