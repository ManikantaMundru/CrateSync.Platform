using CrateSync.Platform.Api.Extensions;
using CrateSync.Platform.Api.Requests.InboundShipments;
using CrateSync.Platform.Api.Responses.InboundShipments;
using CrateSync.Platform.InboundLogistics.Application.Shipments.AddShipmentLine;
using CrateSync.Platform.InboundLogistics.Application.Shipments.CancelShipment;
using CrateSync.Platform.InboundLogistics.Application.Shipments.CreateShipment;
using CrateSync.Platform.InboundLogistics.Application.Shipments.GetShipment;
using CrateSync.Platform.InboundLogistics.Application.Shipments.MarkShipmentArrived;
using CrateSync.Platform.InboundLogistics.Application.Shipments.MarkShipmentShipped;
using CrateSync.Platform.InboundLogistics.Application.Shipments.ReceiveShipment;
using CrateSync.Platform.InboundLogistics.Application.Shipments.SearchShipments;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrateSync.Platform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InboundShipmentController : ControllerBase
{
    private readonly ISender _sender;

    public InboundShipmentController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType<CreateInboundShipmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateInboundShipmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateShipmentCommand(
            request.SupplierId,
            request.WarehouseId,
            request.TrackingNumber,
            request.OriginCountry,
            request.ExpectedArrivalAtUtc);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return CreatedAtAction(
            nameof(GetById),
            new { shipmentId = result.Value },
            new CreateInboundShipmentResponse(result.Value));
    }

    [HttpPost("{shipmentId:guid}/lines")]
    [ProducesResponseType<AddShipmentLineResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddLine(
        Guid shipmentId,
        [FromBody] AddShipmentLineRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddShipmentLineCommand(
            shipmentId,
            request.ProductId,
            request.ProductVarietyId,
            request.BatchCode,
            request.ExpectedQuantity,
            request.PackSizeKg,
            request.UnitCost,
            request.Currency);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return StatusCode(
            StatusCodes.Status201Created,
            new AddShipmentLineResponse(result.Value));
    }

    [HttpPatch("{shipmentId:guid}/ship")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkShipped(
        Guid shipmentId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new MarkShipmentShippedCommand(shipmentId),
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return NoContent();
    }

    [HttpPatch("{shipmentId:guid}/arrive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkArrived(
        Guid shipmentId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new MarkShipmentArrivedCommand(shipmentId),
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return NoContent();
    }

    [HttpPost("{shipmentId:guid}/receive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Receive(
        Guid shipmentId,
        [FromBody] ReceiveShipmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReceiveShipmentCommand(
            shipmentId,
            request.ShipmentLineId,
            request.Quantity);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return NoContent();
    }

    [HttpPatch("{shipmentId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        Guid shipmentId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CancelShipmentCommand(shipmentId),
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return NoContent();
    }

    [HttpGet("{shipmentId:guid}")]
    [ProducesResponseType<ShipmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid shipmentId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetShipmentQuery(shipmentId),
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<ShipmentSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? searchTerm,
        [FromQuery] ShipmentStatus? status,
        [FromQuery] Guid? supplierId,
        [FromQuery] Guid? warehouseId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SearchShipmentsQuery(
                searchTerm,
                status,
                supplierId,
                warehouseId),
            cancellationToken);

        return Ok(result);
    }
}
