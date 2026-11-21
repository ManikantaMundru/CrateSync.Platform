using System.Security.Claims;
using CrateSync.Platform.Api.Authorization;
using CrateSync.Platform.Identity.Domain.Authorization;
using CrateSync.Platform.Identity.Infrastructure.Persistence;
using CrateSync.Platform.Tenancy.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrateSync.Platform.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public sealed class MeController(
    IdentityAccessReader identityAccessReader,
    ITenantReadService tenantReadService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        var subject =
            User.FindFirstValue(
                CrateSyncClaimTypes.Subject)
            ?? User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(subject, out var userId))
        {
            return Unauthorized(new
            {
                title = "Invalid authenticated user",
                detail = "The access token does not contain a valid subject identifier."
            });
        }

        var access =
            await identityAccessReader.GetAsync(
                userId,
                cancellationToken);

        if (access is null)
            return Unauthorized();

        var tenants =
            new List<object>();

        foreach (var membership in access.Tenants)
        {
            var tenant =
                await tenantReadService.GetAsync(
                    membership.TenantId,
                    cancellationToken);

            if (tenant is null)
                continue;

            tenants.Add(new
            {
                tenantId = membership.TenantId,
                name = tenant.Name,
                status = tenant.Status,
                roles = membership.Roles
                    .Select(x => x.ToString())
                    .ToArray(),
                warehouses = membership.Warehouses
                    .Select(x => new
                    {
                        warehouseId = x.WarehouseId,
                        role = x.Role.ToString()
                    })
                    .ToArray(),
                path = $"/tenant/{membership.TenantId}"
            });
        }

        var platformRole =
            access.PlatformRole == PlatformRole.None
                ? null
                : access.PlatformRole.ToString();

        var currentTenantId =
            TryGetCurrentTenantId();

        var actingAsPlatformAdmin =
            User.HasClaim(
                CrateSyncClaimTypes.ActingAsPlatformAdmin,
                bool.TrueString);

        object? currentTenant = null;

        if (currentTenantId.HasValue)
        {
            var currentMembership =
                access.Tenants.FirstOrDefault(
                    x => x.TenantId == currentTenantId.Value);

            var canUseCurrentTenant =
                actingAsPlatformAdmin ||
                currentMembership is not null;

            if (canUseCurrentTenant)
            {
                var tenant =
                    await tenantReadService.GetAsync(
                        currentTenantId.Value,
                        cancellationToken);

                if (tenant is not null &&
                    await tenantReadService.IsActiveAsync(
                        tenant.Id,
                        cancellationToken))
                {
                    currentTenant = new
                    {
                        tenantId = tenant.Id,
                        name = tenant.Name,
                        status = tenant.Status,
                        accessMode =
                            actingAsPlatformAdmin
                                ? "SuperAdmin"
                                : "Membership",
                        roles =
                            currentMembership?.Roles
                                .Select(x => x.ToString())
                                .ToArray()
                            ?? Array.Empty<string>(),
                        warehouses =
                            currentMembership?.Warehouses
                                .Select(x => (object)new
                                {
                                    warehouseId = x.WarehouseId,
                                    role = x.Role.ToString()
                                })
                                .ToArray()
                            ?? Array.Empty<object>(),
                        path =
                            $"/tenant/{tenant.Id}"
                    };
                }
            }
        }

        return Ok(new
        {
            userId = access.UserId,
            displayName = access.DisplayName,
            email = access.Email,
            platform = new
            {
                role = platformRole,
                canAccessAdminPortal =
                    access.PlatformRole == PlatformRole.SuperAdmin,
                adminPath = "/admin"
            },
            currentTenantId,
            currentTenant,
            actingAsPlatformAdmin,
            tenants
        });
    }

    private Guid? TryGetCurrentTenantId()
    {
        var value =
            User.FindFirstValue(
                CrateSyncClaimTypes.TenantId);

        return Guid.TryParse(value, out var tenantId)
            ? tenantId
            : null;
    }
}
