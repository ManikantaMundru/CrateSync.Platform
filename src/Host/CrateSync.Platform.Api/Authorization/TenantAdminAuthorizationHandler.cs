using Microsoft.AspNetCore.Authorization;

namespace CrateSync.Platform.Api.Authorization;

public sealed class TenantAdminAuthorizationHandler(
    ICurrentTenantAccessService accessService)
    : AuthorizationHandler<TenantAdminRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantAdminRequirement requirement)
    {
        if (await accessService.IsTenantAdminAsync())
            context.Succeed(requirement);
    }
}
