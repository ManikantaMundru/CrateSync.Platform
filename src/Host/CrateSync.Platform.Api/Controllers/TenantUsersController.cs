using System.Text;
using System.Security.Claims;
using CrateSync.Platform.Api.Authorization;
using CrateSync.Platform.Api.Requests.Identity;
using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.Identity.Domain.Authorization;
using CrateSync.Platform.Identity.Infrastructure.Identity;
using CrateSync.Platform.Identity.Infrastructure.Persistence;
using CrateSync.Platform.Warehousing.Application.Warehouses.GetWarehouse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc;

namespace CrateSync.Platform.Api.Controllers;

[ApiController]
[Route("api/tenant/users")]
[Authorize(Policy = AuthorizationPolicies.TenantAdmin)]
public sealed class TenantUsersController(
    ITenantContext tenantContext,
    TenantUserManagementService tenantUserManagementService,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    ISender sender)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        CancellationToken cancellationToken)
    {
        var users =
            await tenantUserManagementService.GetUsersAsync(
                tenantContext.TenantId,
                cancellationToken);

        return Ok(users.Select(ToResponse));
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrAdd(
        CreateTenantUserRequest request,
        CancellationToken cancellationToken)
    {
        var validation =
            await ValidateAccessAsync(
                request.Roles,
                request.Warehouses,
                cancellationToken);

        if (validation is not null)
            return validation;

        var result =
            await tenantUserManagementService.CreateOrAddAsync(
                tenantContext.TenantId,
                request.Email,
                request.UserName,
                request.FirstName,
                request.LastName,
                request.Roles,
                request.Warehouses
                    .Select(x =>
                        new WarehouseAccessInput(
                            x.WarehouseId,
                            x.Role))
                    .ToArray(),
                cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                title = "Unable to create tenant user",
                errors = result.Errors
            });
        }

        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                userId = result.UserId,
                createdNewIdentity =
                    result.CreatedNewIdentity,
                requiresActivation =
                    result.RequiresActivation,
                activationUrl =
                    result.RequiresActivation &&
                    result.UserId.HasValue
                        ? await BuildActivationUrlAsync(
                            result.UserId.Value)
                        : null,
                detail =
                    result.RequiresActivation
                        ? "The user requires activation before first login."
                        : "The existing CrateSync identity was added to this organisation."
            });
    }

    [HttpPost("{userId:guid}/activation-link")]
    public async Task<IActionResult> CreateActivationLink(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var canManage =
            await tenantUserManagementService
                .CanManageUserAsync(
                    tenantContext.TenantId,
                    userId,
                    cancellationToken);

        if (!canManage)
            return NotFound();

        var user =
            await userManager.FindByIdAsync(
                userId.ToString());

        if (user is null ||
            !user.IsActive)
        {
            return NotFound();
        }

        var requiresActivation =
            !user.EmailConfirmed ||
            !await userManager.HasPasswordAsync(user);

        if (!requiresActivation)
        {
            return Conflict(new
            {
                title = "Account is already activated",
                detail =
                    "This user already has an active password and does not require an activation link."
            });
        }

        var activationUrl =
            await BuildActivationUrlAsync(userId);

        return Ok(new
        {
            activationUrl
        });
    }

    [HttpPut("{userId:guid}/access")]
    public async Task<IActionResult> UpdateAccess(
        Guid userId,
        UpdateTenantUserAccessRequest request,
        CancellationToken cancellationToken)
    {
        if (IsCurrentUser(userId) &&
            !request.Roles.Contains(
                TenantRole.TenantAdmin))
        {
            return Conflict(new
            {
                title = "Cannot remove your own TenantAdmin access",
                detail =
                    "Another TenantAdmin must change your administrator access."
            });
        }

        var validation =
            await ValidateAccessAsync(
                request.Roles,
                request.Warehouses,
                cancellationToken);

        if (validation is not null)
            return validation;

        var updated =
            await tenantUserManagementService.UpdateAccessAsync(
                tenantContext.TenantId,
                userId,
                request.Roles,
                request.Warehouses
                    .Select(x =>
                        new WarehouseAccessInput(
                            x.WarehouseId,
                            x.Role))
                    .ToArray(),
                cancellationToken);

        return updated
            ? NoContent()
            : NotFound();
    }

    [HttpPatch("{userId:guid}/activate")]
    public async Task<IActionResult> Activate(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var updated =
            await tenantUserManagementService
                .SetMembershipActiveAsync(
                    tenantContext.TenantId,
                    userId,
                    true,
                    cancellationToken);

        return updated
            ? NoContent()
            : NotFound();
    }

    [HttpPatch("{userId:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (IsCurrentUser(userId))
        {
            return Conflict(new
            {
                title = "Cannot deactivate your own tenant membership",
                detail =
                    "Another TenantAdmin must deactivate this membership."
            });
        }

        var updated =
            await tenantUserManagementService
                .SetMembershipActiveAsync(
                    tenantContext.TenantId,
                    userId,
                    false,
                    cancellationToken);

        return updated
            ? NoContent()
            : NotFound();
    }

    private async Task<IActionResult?> ValidateAccessAsync(
        IReadOnlyCollection<
            TenantRole> roles,
        IReadOnlyCollection<WarehouseAccessRequest> warehouses,
        CancellationToken cancellationToken)
    {
        if (roles.Count == 0 &&
            warehouses.Count == 0)
        {
            return BadRequest(new
            {
                title = "At least one access assignment is required"
            });
        }

        if (roles.Any(
                role =>
                    role is not TenantRole.TenantAdmin
                        and not TenantRole.Viewer))
        {
            return BadRequest(new
            {
                title = "Invalid tenant role",
                detail =
                    "Tenant roles are limited to TenantAdmin and Viewer."
            });
        }

        if (warehouses.Any(
                warehouse =>
                    warehouse.WarehouseId == Guid.Empty ||
                    warehouse.Role is not WarehouseAccessRole.WarehouseAdmin
                        and not WarehouseAccessRole.WarehouseUser))
        {
            return BadRequest(new
            {
                title = "Invalid warehouse access assignment"
            });
        }

        if (warehouses
            .GroupBy(x => x.WarehouseId)
            .Any(x => x.Count() > 1))
        {
            return BadRequest(new
            {
                title = "A warehouse can only be assigned once per user"
            });
        }

        foreach (var warehouse in warehouses)
        {
            var result =
                await sender.Send(
                    new GetWarehouseQuery(
                        warehouse.WarehouseId),
                    cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new
                {
                    title = "Invalid warehouse assignment",
                    warehouseId =
                        warehouse.WarehouseId
                });
            }
        }

        return null;
    }

    private async Task<string?> BuildActivationUrlAsync(
        Guid userId)
    {
        var user =
            await userManager.FindByIdAsync(
                userId.ToString());

        if (user is null)
            return null;

        var token =
            await userManager
                .GeneratePasswordResetTokenAsync(
                    user);

        var encodedToken =
            WebEncoders.Base64UrlEncode(
                Encoding.UTF8.GetBytes(token));

        var authority =
            configuration["Identity:Authority"]
            ?? "https://localhost:7201";

        return
            $"{authority.TrimEnd('/')}/Account/Activate" +
            $"?userId={user.Id:D}" +
            $"&token={encodedToken}";
    }

    private bool IsCurrentUser(Guid userId)
    {
        var subject =
            User.FindFirstValue(
                CrateSyncClaimTypes.Subject)
            ?? User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return Guid.TryParse(
                   subject,
                   out var currentUserId) &&
               currentUserId == userId;
    }

    private static object ToResponse(
        TenantUserSummary user)
    {
        return new
        {
            userId = user.UserId,
            userName = user.UserName,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            isActive = user.IsMembershipActive,
            requiresActivation =
                user.RequiresActivation,
            roles = user.Roles
                .Select(x => x.ToString())
                .ToArray(),
            warehouses = user.Warehouses
                .Select(x => new
                {
                    warehouseId = x.WarehouseId,
                    role = x.Role.ToString()
                })
                .ToArray()
        };
    }
}
