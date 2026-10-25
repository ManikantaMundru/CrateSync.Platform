using CrateSync.Platform.Api.Extensions;
using CrateSync.Platform.Api.Requests.Warehouse;
using CrateSync.Platform.Api.Responses.Warehouse;
using CrateSync.Platform.Warehousing.Application.Warehouses.ActivateWarehouse;
using CrateSync.Platform.Warehousing.Application.Warehouses.CreateWarehouse;
using CrateSync.Platform.Warehousing.Application.Warehouses.DeactivateWarehouse;
using CrateSync.Platform.Warehousing.Application.Warehouses.GetWarehouse;
using CrateSync.Platform.Warehousing.Application.Warehouses.SearchWarehouses;
using CrateSync.Platform.Warehousing.Application.Warehouses.UpdateWarehouse;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrateSync.Platform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WarehouseController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpPost]
    [ProducesResponseType<CreateWarehouseResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateWarehouseCommand(
            request.Name,
            request.Code,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.StateOrProvince,
            request.PostalCode,
            request.Country);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure) return this.ToProblem(result.Error);

        return CreatedAtAction(
            nameof(GetById),
            new { warehouseId = result.Value },
            new CreateWarehouseResponse(result.Value));
    }

    [HttpPut("{warehouseId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid warehouseId,
        [FromBody] UpdateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateWarehouseCommand(
            warehouseId,
            request.Name,
            request.Code,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.StateOrProvince,
            request.PostalCode,
            request.Country);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure) return this.ToProblem(result.Error);

        return NoContent();
    }

    [HttpGet("{warehouseId:guid}")]
    [ProducesResponseType<WarehouseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetWarehouseQuery(warehouseId),
            cancellationToken);

        if (result.IsFailure) return this.ToProblem(result.Error);

        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<WarehouseSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SearchWarehousesQuery(search, isActive),
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{warehouseId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ActivateWarehouseCommand(warehouseId),
            cancellationToken);

        if (result.IsFailure) return this.ToProblem(result.Error);

        return NoContent();
    }

    [HttpPatch("{warehouseId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new DeactivateWarehouseCommand(warehouseId),
            cancellationToken);

        if (result.IsFailure) return this.ToProblem(result.Error);

        return NoContent();
    }
}
