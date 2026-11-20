using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.Tenancy.Application.Abstractions;

namespace CrateSync.Platform.Api.Tenancy;

public sealed class TenantValidationMiddleware(
    RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        ITenantReadService tenantReadService)
    {
        var path = context.Request.Path;

        // Tenant administration may need to work without
        // an existing tenant context.
        if (path.StartsWithSegments("/api/tenants") ||
            path.StartsWithSegments("/swagger") ||
            path.StartsWithSegments("/health"))
        {
            await next(context);
            return;
        }

        if (!tenantContext.HasTenant)
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsJsonAsync(new
            {
                title = "Tenant context is missing",
                detail =
                    "The authenticated user does not contain a valid tenant_id claim."
            });

            return;
        }

        var isActive = await tenantReadService.IsActiveAsync(
            tenantContext.TenantId,
            context.RequestAborted);

        if (!isActive)
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            await context.Response.WriteAsJsonAsync(new
            {
                title = "Tenant is inactive",
                detail =
                    "Operational activity is not allowed for this tenant."
            });

            return;
        }

        await next(context);
    }
}
