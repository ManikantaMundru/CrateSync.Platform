using System.Security.Claims;
using CrateSync.Platform.Inventory.Application.Stock.SetSellingPrice;
using CrateSync.Platform.Identity.Domain.Authorization;
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
        CancellationToken cancellationToken, [FromQuery] Guid batchId = default)
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
                originCountry, batchId),
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
        CancellationToken cancellationToken, [FromQuery] Guid batchId = default)
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
                originCountry, batchId),
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
                request.Remarks, request.BatchId, request.TransportCompany, request.DriverName, request.DriverPhone, request.VehicleNumber),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpGet("transfers")]
    public async Task<IActionResult> ListTransfers(CancellationToken cancellationToken)
    {
        var rows = await sender.Send(new ListWarehouseTransfersQuery(), cancellationToken);
        if (await accessService.HasTenantWideReadAsync(cancellationToken)) return Ok(rows);
        var assigned = await accessService.GetAssignedWarehouseIdsAsync(cancellationToken);
        return Ok(rows.Where(x => assigned.Contains(x.SourceWarehouseId) || assigned.Contains(x.DestinationWarehouseId)));
    }

    [HttpPatch("transfers/{id:guid}/{action}")]
    public async Task<IActionResult> TransitionTransfer(Guid id, string action, [FromBody] TransferInspectionRequest? request, CancellationToken cancellationToken)
    {
        if (action is not ("dispatch" or "cancel" or "receive")) return BadRequest();
        var transfer = await sender.Send(new GetWarehouseTransferQuery(id), cancellationToken);
        if (transfer is null) return NotFound();
        var warehouseId = action == "receive" ? transfer.DestinationWarehouseId : transfer.SourceWarehouseId;
        if (!await accessService.CanAdminWarehouseAsync(warehouseId, cancellationToken)) return Forbid();
        if (action == "receive" && request is null) return BadRequest();
        var result = await sender.Send(new TransitionWarehouseTransferCommand(id, action, request?.Accepted ?? 0, request?.Damaged ?? 0, request?.Missing ?? 0, request?.Remarks), cancellationToken);
        return result.IsFailure ? this.ToProblem(result.Error) : NoContent();
    }

    public sealed record TransferInspectionRequest(int Accepted, int Damaged, int Missing, string? Remarks);

    [HttpPut("stock/{balanceId:guid}/selling-price")]
    public async Task<IActionResult> SetSellingPrice(Guid balanceId, [FromBody] SellingPriceRequest request, CancellationToken cancellationToken)
    {
        if (!await accessService.CanAdminWarehouseAsync(request.WarehouseId, cancellationToken)) return Forbid();
        var subject = User.FindFirstValue(CrateSyncClaimTypes.Subject) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out var userId)) return Forbid();
        var result = await sender.Send(new SetSellingPriceCommand(balanceId, request.WarehouseId, request.Price, request.Currency, userId, request.Reason), cancellationToken);
        return result.IsFailure ? this.ToProblem(result.Error) : NoContent();
    }

    [HttpGet("stock/{balanceId:guid}/selling-price/history")]
    public async Task<IActionResult> GetSellingPriceHistory(Guid balanceId, [FromQuery] Guid warehouseId, CancellationToken cancellationToken)
    {
        if (!await accessService.CanReadWarehouseAsync(warehouseId, cancellationToken)) return Forbid();
        var history = await sender.Send(new GetSellingPriceHistoryQuery(balanceId, warehouseId), cancellationToken);
        return history is null ? NotFound() : Ok(history);
    }

    public sealed record SellingPriceRequest(Guid WarehouseId, decimal Price, string Currency, string Reason);

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
