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
    private readonly ISender _sender = sender;

    [HttpGet("stock")]
    [ProducesResponseType<StockResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStock(
        [FromQuery] Guid warehouseId,
        [FromQuery] Guid productVarietyId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetStockQuery(
                warehouseId,
                productVarietyId),
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<StockSummaryResponse>>(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productVarietyId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SearchStockQuery(
                warehouseId,
                productVarietyId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("movements")]
    [ProducesResponseType<IReadOnlyCollection<StockMovementResponse>>(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovements(
        [FromQuery] Guid warehouseId,
        [FromQuery] Guid productVarietyId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetStockMovementsQuery(
                warehouseId,
                productVarietyId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("adjustments")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Adjust(
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AdjustStockCommand(
                request.WarehouseId,
                request.ProductVarietyId,
                request.QuantityChange,
                request.Reason),
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return NoContent();
    }
}
