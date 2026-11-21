using CrateSync.Platform.Api.Authorization;
using CrateSync.Platform.Api.Extensions;
using CrateSync.Platform.Api.Requests.Tenancy;
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
[Authorize(Policy = AuthorizationPolicies.PlatformAdmin)]
public sealed class TenantController(
    ISender sender)
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
