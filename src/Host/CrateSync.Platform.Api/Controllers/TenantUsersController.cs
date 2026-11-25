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
    ISender sender,
    ICurrentTenantAccessService accessService)
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

        var existingUser = await userManager.FindByEmailAsync(request.Email.Trim());
        if (existingUser is not null &&
            !await CanManageTargetAsync(existingUser.Id, cancellationToken))
            return Forbid();

        if (existingUser is not null)
        {
            var members = await tenantUserManagementService.GetUsersAsync(
                tenantContext.TenantId, cancellationToken);
            if (members.Any(x => x.UserId == existingUser.Id))
                return Conflict(new { title = "User already belongs to this tenant", detail = "Use Edit access to change an existing membership." });
        }

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
        if (!await CanManageTargetAsync(userId, cancellationToken))
            return Forbid();

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
        if (!await CanManageTargetAsync(userId, cancellationToken))
            return Forbid();

        if (IsCurrentUser(userId))
            return Conflict(new { title = "Cannot change your own access", detail = "Another authorized administrator must change your access." });

        if (!request.Roles.Contains(TenantRole.TenantOwner) &&
            await IsLastActiveOwnerAsync(userId, cancellationToken))
            return Conflict(new { title = "Cannot remove the last active tenant owner" });

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
        if (!await CanManageTargetAsync(userId, cancellationToken))
            return Forbid();

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
        if (!await CanManageTargetAsync(userId, cancellationToken))
            return Forbid();

        if (IsCurrentUser(userId))
        {
            return Conflict(new
            {
                title = "Cannot deactivate your own tenant membership",
                detail =
                    "Another TenantAdmin must deactivate this membership."
            });
        }

        if (await IsLastActiveOwnerAsync(userId, cancellationToken))
            return Conflict(new { title = "Cannot deactivate the last active tenant owner" });

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
                    !Enum.IsDefined(role)))
        {
            return BadRequest(new
            {
                title = "Invalid tenant role",
                detail =
                    "Unknown tenant role."
            });
        }

        if (roles.Any(TenantPermissions.IsFinancialRole) &&
            !await accessService.HasPermissionAsync(TenantPermission.ManageFinancialRoles, cancellationToken))
            return Forbid();

        if (roles.Count > 1 || (roles.Count != 0 && warehouses.Count != 0))
            return BadRequest(new { title = "Select one primary role. Tenant-wide roles cannot have warehouse assignments." });

        if (warehouses.Select(x => x.Role).Distinct().Count() > 1)
            return BadRequest(new { title = "Use the same warehouse role for every assignment." });

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

            if (result.IsFailure || !result.Value.IsActive)
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

    private async Task<bool> IsLastActiveOwnerAsync(Guid userId, CancellationToken cancellationToken)
    {
        var users = await tenantUserManagementService.GetUsersAsync(tenantContext.TenantId, cancellationToken);
        var owners = users.Where(x => x.IsMembershipActive && x.Roles.Contains(TenantRole.TenantOwner)).ToArray();
        return owners.Length == 1 && owners[0].UserId == userId;
    }

    private async Task<bool> CanManageTargetAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (await accessService.HasPermissionAsync(TenantPermission.ManageFinancialRoles, cancellationToken))
            return true;

        var users = await tenantUserManagementService.GetUsersAsync(tenantContext.TenantId, cancellationToken);
        return !users.Any(x => x.UserId == userId && x.Roles.Any(TenantPermissions.IsFinancialRole));
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
