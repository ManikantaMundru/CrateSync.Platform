using CrateSync.Platform.Api.Authorization;
using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.Tenancy.Application.Abstractions;

namespace CrateSync.Platform.Api.Tenancy;

public sealed class TenantValidationMiddleware(
    RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        ITenantReadService tenantReadService,
        ICurrentTenantAccessService accessService)
    {
        var path = context.Request.Path;

        if (path == "/" ||
            path.StartsWithSegments("/swagger") ||
            path.StartsWithSegments("/health") ||
            path.StartsWithSegments("/api/me") ||
            path.StartsWithSegments("/api/platform") ||
            path.StartsWithSegments("/api/tenants"))
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

        if (!await accessService.HasTenantAccessAsync(
                context.RequestAborted))
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            await context.Response.WriteAsJsonAsync(new
            {
                title = "Tenant access denied",
                detail =
                    "The authenticated user does not have an active membership for this tenant."
            });

            return;
        }

        await next(context);
    }
}
