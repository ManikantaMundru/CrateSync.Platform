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
                productVarietyId),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productVarietyId,
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
                productVarietyId),
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
                productVarietyId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("transfers")]
    public async Task<IActionResult> Transfer(
        [FromBody] TransferStockRequest request,
        CancellationToken cancellationToken)
    {
        if (!await accessService.CanOperateWarehouseAsync(
                request.SourceWarehouseId,
                cancellationToken) ||
            !await accessService.CanOperateWarehouseAsync(
                request.DestinationWarehouseId,
                cancellationToken))
        {
            return Forbid();
        }

        var result = await sender.Send(
            new TransferStockCommand(
                request.TransferId,
                request.SourceWarehouseId,
                request.DestinationWarehouseId,
                request.ProductVarietyId,
                request.Quantity,
                request.TransferReference,
                request.Remarks),
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
                request.QuantityChange,
                request.Reason),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }
}
