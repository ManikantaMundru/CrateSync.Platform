using CrateSync.Platform.Warehousing.Application.Warehouses.GetWarehouse;
using CrateSync.Platform.Warehousing.Application.Warehouses.SearchWarehouses;
using CrateSync.Platform.Api.Authorization;
using CrateSync.Platform.Api.Extensions;
using CrateSync.Platform.Api.Requests.Inventory;
using CrateSync.Platform.Inventory.Application.Stock.AdjustStock;
using CrateSync.Platform.Inventory.Application.Stock.GetStock;
using CrateSync.Platform.Inventory.Application.Stock.GetStockMovements;
using CrateSync.Platform.Inventory.Application.Stock.SearchStock;
using CrateSync.Platform.Inventory.Application.Stock.TransferStock;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrateSync.Platform.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController(
    ISender sender,
    ICurrentTenantAccessService accessService)
    : ControllerBase
{
    [HttpGet("stock")]
    public async Task<IActionResult> GetStock(
        [FromQuery] Guid warehouseId,
        [FromQuery] Guid productVarietyId,
        [FromQuery] string originCountry,
        CancellationToken cancellationToken)
    {
        if (!await accessService.CanReadWarehouseAsync(
                warehouseId,
                cancellationToken))
        {
            return Forbid();
        }

        var result = await sender.Send(
            new GetStockQuery(
                warehouseId,
                productVarietyId,
                originCountry),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productVarietyId,
        [FromQuery] string? originCountry,
        CancellationToken cancellationToken)
    {
        if (warehouseId.HasValue &&
            !await accessService.CanReadWarehouseAsync(
                warehouseId.Value,
                cancellationToken))
        {
            return Forbid();
        }

        var result = await sender.Send(
            new SearchStockQuery(
                warehouseId,
                productVarietyId,
                originCountry),
            cancellationToken);

        if (warehouseId.HasValue ||
            await accessService.HasTenantWideReadAsync(
                cancellationToken))
        {
            return Ok(result);
        }

        var assignedWarehouseIds =
            await accessService.GetAssignedWarehouseIdsAsync(
                cancellationToken);

        return Ok(
            result.Where(
                x =>
                    assignedWarehouseIds.Contains(
                        x.WarehouseId)));
    }

    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements(
        [FromQuery] Guid warehouseId,
        [FromQuery] Guid productVarietyId,
        [FromQuery] string originCountry,
        CancellationToken cancellationToken)
    {
        if (!await accessService.CanReadWarehouseAsync(
                warehouseId,
                cancellationToken))
        {
            return Forbid();
        }

        var result = await sender.Send(
            new GetStockMovementsQuery(
                warehouseId,
                productVarietyId,
                originCountry),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("transfer-destinations")]
    public async Task<IActionResult> GetTransferDestinations(CancellationToken cancellationToken)
    {
        var assigned = await accessService.GetAssignedWarehouseIdsAsync(cancellationToken);
        var allowed = await accessService.IsTenantAdminAsync(cancellationToken);
        foreach (var id in assigned)
            allowed |= await accessService.CanAdminWarehouseAsync(id, cancellationToken);
        if (!allowed) return Forbid();

        var warehouses = await sender.Send(new SearchWarehousesQuery(null, true), cancellationToken);
        return Ok(warehouses.Select(x => new { x.Id, x.Name, x.Code, x.IsActive }));
    }

    [HttpPost("transfers")]
    public async Task<IActionResult> Transfer(
        [FromBody] TransferStockRequest request,
        CancellationToken cancellationToken)
    {
        if (!await accessService.CanAdminWarehouseAsync(
                request.SourceWarehouseId,
                cancellationToken))
        {
            return Forbid();
        }

        var destination = await sender.Send(new GetWarehouseQuery(request.DestinationWarehouseId), cancellationToken);
        if (destination.IsFailure) return this.ToProblem(destination.Error);
        if (!destination.Value.IsActive) return BadRequest(new { title = "Destination warehouse is inactive" });

        var result = await sender.Send(
            new TransferStockCommand(
                request.TransferId,
                request.SourceWarehouseId,
                request.DestinationWarehouseId,
                request.ProductVarietyId,
                request.OriginCountry,
                request.Quantity,
                request.TransferReference,
                request.Remarks, request.BatchId),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpPost("adjustments")]
    public async Task<IActionResult> Adjust(
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        if (!await accessService.CanAdminWarehouseAsync(
                request.WarehouseId,
                cancellationToken))
        {
            return Forbid();
        }

        var result = await sender.Send(
            new AdjustStockCommand(
                request.WarehouseId,
                request.ProductVarietyId,
                request.OriginCountry,
                request.QuantityChange,
                request.Reason, request.BatchId),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }
}
