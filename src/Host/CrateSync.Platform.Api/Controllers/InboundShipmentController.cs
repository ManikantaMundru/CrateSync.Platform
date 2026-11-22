using CrateSync.Platform.Api.Authorization;
using CrateSync.Platform.Api.Extensions;
using CrateSync.Platform.Api.Requests.InboundShipments;
using CrateSync.Platform.Api.Responses.InboundShipments;
using CrateSync.Platform.InboundLogistics.Application.Shipments.AddShipmentDocument;
using CrateSync.Platform.InboundLogistics.Application.Shipments.AddShipmentLine;
using CrateSync.Platform.InboundLogistics.Application.Shipments.CancelShipment;
using CrateSync.Platform.InboundLogistics.Application.Shipments.CreateShipment;
using CrateSync.Platform.InboundLogistics.Application.Shipments.GetShipment;
using CrateSync.Platform.InboundLogistics.Application.Shipments.MarkShipmentArrived;
using CrateSync.Platform.InboundLogistics.Application.Shipments.MarkShipmentShipped;
using CrateSync.Platform.InboundLogistics.Application.Shipments.ReceiveShipment;
using CrateSync.Platform.InboundLogistics.Application.Shipments.RecordClearanceEvent;
using CrateSync.Platform.InboundLogistics.Application.Shipments.SearchShipments;
using CrateSync.Platform.InboundLogistics.Application.Shipments.UpdateShipmentReferences;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrateSync.Platform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InboundShipmentController(
    ISender sender,
    ICurrentTenantAccessService accessService)
    : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateInboundShipmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateInboundShipmentRequest request,
        CancellationToken cancellationToken)
    {
        if (!await accessService.HasTenantAccessAsync(
                cancellationToken))
        {
            return Forbid();
        }

        var command = new CreateShipmentCommand(
            request.SupplierId,
            request.TrackingNumber,
            request.OriginCountry,
            request.ExpectedArrivalAtUtc);

        var result = await sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return CreatedAtAction(
            nameof(GetById),
            new { shipmentId = result.Value },
            new CreateInboundShipmentResponse(
                result.Value));
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
        var accessResult =
            await EnsureShipmentOperationAccessAsync(
                shipmentId,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var command = new AddShipmentLineCommand(
            shipmentId,
            request.ProductId,
            request.ProductVarietyId,
            request.BatchCode,
            request.ExpectedQuantity,
            request.PackSizeKg,
            request.UnitCost,
            request.Currency);

        var result = await sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return StatusCode(
            StatusCodes.Status201Created,
            new AddShipmentLineResponse(result.Value));
    }


    [HttpPut("{shipmentId:guid}/references")]
    public async Task<IActionResult> UpdateReferences(
        Guid shipmentId,
        [FromBody] UpdateShipmentReferencesRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult =
            await EnsureShipmentOperationAccessAsync(
                shipmentId,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var command = new UpdateShipmentReferencesCommand(
            shipmentId,
            request.ContainerNumber,
            request.BillOfLadingNumber,
            request.PortOfLoading,
            request.PortOfDischarge,
            request.VesselName,
            request.VoyageNumber,
            request.CustomsBrokerName,
            request.BillOfEntryNumber,
            request.BillOfEntryDate,
            request.OutOfChargeAtUtc);

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpPost("{shipmentId:guid}/clearance-events")]
    [ProducesResponseType<CreateShipmentClearanceEventResponse>(
        StatusCodes.Status201Created)]
    public async Task<IActionResult> RecordClearanceEvent(
        Guid shipmentId,
        [FromBody] RecordClearanceEventRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult =
            await EnsureShipmentOperationAccessAsync(
                shipmentId,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(
            new RecordClearanceEventCommand(
                shipmentId,
                request.Authority,
                request.Status,
                request.ReferenceNumber,
                request.Remarks),
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return StatusCode(
            StatusCodes.Status201Created,
            new CreateShipmentClearanceEventResponse(
                result.Value));
    }

    [HttpPost("{shipmentId:guid}/documents")]
    [ProducesResponseType<CreateShipmentDocumentResponse>(
        StatusCodes.Status201Created)]
    public async Task<IActionResult> AddDocument(
        Guid shipmentId,
        [FromBody] AddShipmentDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult =
            await EnsureShipmentOperationAccessAsync(
                shipmentId,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(
            new AddShipmentDocumentCommand(
                shipmentId,
                request.Type,
                request.DocumentName,
                request.ReferenceNumber,
                request.Notes),
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return StatusCode(
            StatusCodes.Status201Created,
            new CreateShipmentDocumentResponse(
                result.Value));
    }

    [HttpPatch("{shipmentId:guid}/ship")]
    public async Task<IActionResult> MarkShipped(
        Guid shipmentId,
        CancellationToken cancellationToken)
    {
        var accessResult =
            await EnsureShipmentOperationAccessAsync(
                shipmentId,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(
            new MarkShipmentShippedCommand(
                shipmentId),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpPatch("{shipmentId:guid}/arrive")]
    public async Task<IActionResult> MarkArrived(
        Guid shipmentId,
        CancellationToken cancellationToken)
    {
        var accessResult =
            await EnsureShipmentOperationAccessAsync(
                shipmentId,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(
            new MarkShipmentArrivedCommand(
                shipmentId),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpPost("{shipmentId:guid}/receive")]
    public async Task<IActionResult> Receive(
        Guid shipmentId,
        [FromBody] ReceiveShipmentRequest request,
        CancellationToken cancellationToken)
    {
        if (!await accessService.CanOperateWarehouseAsync(
                request.WarehouseId,
                cancellationToken))
        {
            return Forbid();
        }

        var command = new ReceiveShipmentCommand(
            request.ReceiptId,
            shipmentId,
            request.ShipmentLineId,
            request.WarehouseId,
            request.Quantity);

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpPatch("{shipmentId:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid shipmentId,
        CancellationToken cancellationToken)
    {
        var accessResult =
            await EnsureShipmentOperationAccessAsync(
                shipmentId,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(
            new CancelShipmentCommand(
                shipmentId),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpGet("{shipmentId:guid}")]
    public async Task<IActionResult> GetById(
        Guid shipmentId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetShipmentQuery(shipmentId),
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return await accessService.HasTenantAccessAsync(
                cancellationToken)
            ? Ok(result.Value)
            : Forbid();
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? searchTerm,
        [FromQuery] ShipmentStatus? status,
        [FromQuery] Guid? supplierId,
        CancellationToken cancellationToken)
    {
        if (!await accessService.HasTenantAccessAsync(
                cancellationToken))
        {
            return Forbid();
        }

        var result = await sender.Send(
            new SearchShipmentsQuery(
                searchTerm,
                status,
                supplierId),
            cancellationToken);

        return Ok(result);
    }

    private async Task<IActionResult?>
        EnsureShipmentOperationAccessAsync(
            Guid shipmentId,
            CancellationToken cancellationToken)
    {
        var shipment =
            await sender.Send(
                new GetShipmentQuery(
                    shipmentId),
                cancellationToken);

        if (shipment.IsFailure)
            return this.ToProblem(
                shipment.Error);

        return await accessService.HasTenantAccessAsync(
                cancellationToken)
            ? null
            : Forbid();
    }
}
