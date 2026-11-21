using CrateSync.Platform.Api.Authorization;
using CrateSync.Platform.Api.Extensions;
using CrateSync.Platform.Api.Requests.Tenancy;
using CrateSync.Platform.Identity.Domain.Authorization;
using System.Text;
using CrateSync.Platform.Identity.Infrastructure.Identity;
using CrateSync.Platform.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using CrateSync.Platform.Tenancy.Application.Tenants.ActivateTenant;
using CrateSync.Platform.Tenancy.Application.Tenants.CreateTenant;
using CrateSync.Platform.Tenancy.Application.Tenants.DeactivateTenant;
using CrateSync.Platform.Tenancy.Application.Tenants.GetTenant;
using CrateSync.Platform.Tenancy.Application.Tenants.RenameTenant;
using CrateSync.Platform.Tenancy.Application.Tenants.SearchTenants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrateSync.Platform.Api.Controllers;

[ApiController]
[Route("api/tenants")]
[Authorize(Policy = AuthorizationPolicies.SuperAdmin)]
public sealed class TenantController(
    ISender sender,
    TenantUserManagementService tenantUserManagementService,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTenantCommand(request.Name),
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value },
            result.Value);
    }

    [HttpPost("{id:guid}/administrators")]
    public async Task<IActionResult> CreateInitialAdministrator(
        Guid id,
        CreateInitialTenantAdminRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest(new
            {
                title = "Administrator details are required",
                detail = "Email, first name and last name are required."
            });
        }

        var tenant =
            await sender.Send(
                new GetTenantQuery(id),
                cancellationToken);

        if (tenant.IsFailure)
            return this.ToProblem(tenant.Error);

        var result =
            await tenantUserManagementService.CreateOrAddAsync(
                id,
                request.Email,
                request.UserName,
                request.FirstName,
                request.LastName,
                [TenantRole.TenantAdmin],
                Array.Empty<WarehouseAccessInput>(),
                cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                title = "Unable to create tenant administrator",
                errors = result.Errors
            });
        }

        string? activationUrl = null;

        if (result.RequiresActivation &&
            result.UserId.HasValue)
        {
            activationUrl =
                await BuildActivationUrlAsync(
                    result.UserId.Value);
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
                activationUrl
            });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Rename(
        Guid id,
        RenameTenantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RenameTenantCommand(
                id,
                request.Name),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ActivateTenantCommand(id),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeactivateTenantCommand(id),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetTenantQuery(id),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SearchTenantsQuery(isActive),
            cancellationToken);

        return Ok(result);
    }
}
