using CrateSync.Platform.Api.Extensions;
using CrateSync.Platform.Api.Requests.Inventory;
using CrateSync.Platform.Inventory.Application.Stock.AdjustStock;
using CrateSync.Platform.Inventory.Application.Stock.GetStock;
using CrateSync.Platform.Inventory.Application.Stock.GetStockMovements;
using CrateSync.Platform.Inventory.Application.Stock.SearchStock;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrateSync.Platform.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController(ISender sender) : ControllerBase
{
    [HttpGet("stock")]
    public async Task<IActionResult> GetStock(
        [FromQuery] Guid warehouseId,
        [FromQuery] Guid productVarietyId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetStockQuery(warehouseId, productVarietyId),
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
        var result = await sender.Send(
            new SearchStockQuery(warehouseId, productVarietyId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements(
        [FromQuery] Guid warehouseId,
        [FromQuery] Guid productVarietyId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetStockMovementsQuery(warehouseId, productVarietyId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("adjustments")]
    public async Task<IActionResult> Adjust(
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
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
