using CrateSync.Platform.InboundLogistics.Application.Shipments.SetShipmentLinePurchaseCost;
using CrateSync.Platform.InboundLogistics.Application.Shipments.MarkWarehouseDeliveryDelivered;
using CrateSync.Platform.Identity.Domain.Authorization;
using CrateSync.Platform.Api.Authorization;
using CrateSync.Platform.Api.Extensions;
using CrateSync.Platform.Api.Requests.InboundShipments;
using CrateSync.Platform.Api.Responses.InboundShipments;
using CrateSync.Platform.InboundLogistics.Application.Shipments.AddShipmentDocument;
using CrateSync.Platform.InboundLogistics.Application.Shipments.AddShipmentLine;
using CrateSync.Platform.InboundLogistics.Application.Shipments.CancelShipment;
using CrateSync.Platform.InboundLogistics.Application.Shipments.CreateShipment;
using CrateSync.Platform.InboundLogistics.Application.Shipments.CreateWarehouseDelivery;
using CrateSync.Platform.InboundLogistics.Application.Shipments.DispatchWarehouseDelivery;
using CrateSync.Platform.InboundLogistics.Application.Shipments.ConfirmWarehouseDelivery;
using CrateSync.Platform.InboundLogistics.Application.Shipments.GetShipment;
using CrateSync.Platform.InboundLogistics.Application.Shipments.MarkShipmentArrived;
using CrateSync.Platform.InboundLogistics.Application.Shipments.MarkShipmentShipped;
using CrateSync.Platform.InboundLogistics.Application.Shipments.RecordClearanceEvent;
using CrateSync.Platform.InboundLogistics.Application.Shipments.SearchShipments;
using CrateSync.Platform.InboundLogistics.Application.Shipments.UpdateShipmentReferences;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrateSync.Platform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InboundShipmentController(ISender sender, ICurrentTenantAccessService accessService)
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
        if (!await accessService.IsTenantAdminAsync(cancellationToken))
        {
            return Forbid();
        }

        var command = new CreateShipmentCommand(
            request.SupplierId,
            request.TrackingNumber,
            request.OriginCountry,
            request.ExpectedArrivalAtUtc);

        var result = await sender.Send(command, cancellationToken);

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
        var accessResult = await EnsureShipmentOperationAccessAsync(shipmentId, cancellationToken);

        if (accessResult is not null)
            return accessResult;

        // Operational line creation is permitted without costs; supplied costs require financial permission.
        if ((request.UnitCost.HasValue || request.Currency is not null) &&
            !await accessService.HasPermissionAsync(TenantPermission.ManageCosts, cancellationToken))
            return Forbid();

        var command = new AddShipmentLineCommand(
            shipmentId,
            request.ProductId,
            request.ProductVarietyId,
            request.BatchCode,
            request.ExpectedQuantity,
            request.PackSizeKg,
            request.UnitCost,
            request.Currency,
            request.FruitsPerPack);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return StatusCode(StatusCodes.Status201Created, new AddShipmentLineResponse(result.Value));
    }

    [HttpPut("{shipmentId:guid}/lines/{lineId:guid}/purchase-cost")]
    public async Task<IActionResult> SetPurchaseCost(Guid shipmentId, Guid lineId,
        [FromBody] SetShipmentLinePurchaseCostRequest request, CancellationToken cancellationToken)
    {
        if (!await accessService.HasPermissionAsync(TenantPermission.ManageCosts, cancellationToken))
            return Forbid();

        var result = await sender.Send(new SetShipmentLinePurchaseCostCommand(
            shipmentId, lineId, request.UnitCost, request.Currency), cancellationToken);
        return result.IsFailure ? this.ToProblem(result.Error) : NoContent();
    }

    [HttpPut("{shipmentId:guid}/references")]
    public async Task<IActionResult> UpdateReferences(
        Guid shipmentId,
        [FromBody] UpdateShipmentReferencesRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult = await EnsureShipmentOperationAccessAsync(shipmentId, cancellationToken);

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

        var result = await sender.Send(command, cancellationToken);

        return result.IsFailure ? this.ToProblem(result.Error) : NoContent();
    }

    [HttpPost("{shipmentId:guid}/clearance-events")]
    [ProducesResponseType<CreateShipmentClearanceEventResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> RecordClearanceEvent(
        Guid shipmentId,
        [FromBody] RecordClearanceEventRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult = await EnsureShipmentOperationAccessAsync(shipmentId, cancellationToken);

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

        return StatusCode(StatusCodes.Status201Created, new CreateShipmentClearanceEventResponse(result.Value));
    }

    [HttpPost("{shipmentId:guid}/documents")]
    [ProducesResponseType<CreateShipmentDocumentResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddDocument(
        Guid shipmentId,
        [FromBody] AddShipmentDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult = await EnsureShipmentOperationAccessAsync(shipmentId, cancellationToken);

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

        return StatusCode(StatusCodes.Status201Created, new CreateShipmentDocumentResponse(result.Value));
    }

    [HttpPatch("{shipmentId:guid}/ship")]
    public async Task<IActionResult> MarkShipped(Guid shipmentId, CancellationToken cancellationToken)
    {
        var accessResult = await EnsureShipmentOperationAccessAsync(shipmentId, cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(new MarkShipmentShippedCommand(shipmentId), cancellationToken);

        return result.IsFailure ? this.ToProblem(result.Error) : NoContent();
    }

    [HttpPatch("{shipmentId:guid}/arrive")]
    public async Task<IActionResult> MarkArrived(Guid shipmentId, CancellationToken cancellationToken)
    {
        var accessResult = await EnsureShipmentOperationAccessAsync(shipmentId, cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(new MarkShipmentArrivedCommand(shipmentId), cancellationToken);

        return result.IsFailure ? this.ToProblem(result.Error) : NoContent();
    }

    [HttpPost("{shipmentId:guid}/warehouse-deliveries")]
    public async Task<IActionResult> CreateWarehouseDelivery(
        Guid shipmentId,
        [FromBody] CreateWarehouseDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        if (!await accessService.IsTenantAdminAsync(cancellationToken))
        {
            return Forbid();
        }

        var result = await sender.Send(
            new CreateWarehouseDeliveryCommand(
                shipmentId,
                request.WarehouseId,
                request.TransportCompany,
                request.DriverName,
                request.DriverPhone,
                request.VehicleNumber,
                request.OriginLocation,
                request.DestinationLocation,
                request.TransportReference,
                request.Remarks,
                request.Allocations
                    .Select(x => new CreateWarehouseDeliveryAllocation(
                        x.ShipmentLineId,
                        x.Quantity))
                    .ToArray()),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : StatusCode(
                StatusCodes.Status201Created,
                new { id = result.Value });
    }

    [HttpPatch("{shipmentId:guid}/warehouse-deliveries/{deliveryId:guid}/dispatch")]
    public async Task<IActionResult> DispatchWarehouseDelivery(
        Guid shipmentId,
        Guid deliveryId,
        CancellationToken cancellationToken)
    {
        var shipment = await sender.Send(
            new GetShipmentQuery(shipmentId),
            cancellationToken);

        if (shipment.IsFailure)
            return this.ToProblem(shipment.Error);

        var delivery = shipment.Value.Deliveries?
            .SingleOrDefault(x => x.Id == deliveryId);

        if (delivery is null)
            return NotFound();

        if (!await accessService.IsTenantAdminAsync(cancellationToken))
        {
            return Forbid();
        }

        var result = await sender.Send(
            new DispatchWarehouseDeliveryCommand(
                shipmentId,
                deliveryId),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpPatch("{shipmentId:guid}/warehouse-deliveries/{deliveryId:guid}/deliver")]
    public async Task<IActionResult> MarkWarehouseDeliveryDelivered(
        Guid shipmentId, Guid deliveryId, CancellationToken cancellationToken)
    {
        if (!await accessService.IsTenantAdminAsync(cancellationToken))
            return Forbid();

        var result = await sender.Send(
            new MarkWarehouseDeliveryDeliveredCommand(shipmentId, deliveryId),
            cancellationToken);

        return result.IsFailure ? this.ToProblem(result.Error) : NoContent();
    }

    [HttpPatch("{shipmentId:guid}/warehouse-deliveries/{deliveryId:guid}/receive")]
    public async Task<IActionResult> ConfirmWarehouseDelivery(
        Guid shipmentId,
        Guid deliveryId,
        CancellationToken cancellationToken)
    {
        var shipment = await sender.Send(
            new GetShipmentQuery(shipmentId),
            cancellationToken);

        if (shipment.IsFailure)
            return this.ToProblem(shipment.Error);

        var delivery = shipment.Value.Deliveries?
            .SingleOrDefault(x => x.Id == deliveryId);

        if (delivery is null)
            return NotFound();

        if (!await accessService.CanAdminWarehouseAsync(delivery.WarehouseId, cancellationToken))
        {
            return Forbid();
        }

        var result = await sender.Send(
            new ConfirmWarehouseDeliveryCommand(
                shipmentId,
                deliveryId),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpPatch("{shipmentId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid shipmentId, CancellationToken cancellationToken)
    {
        var accessResult = await EnsureShipmentOperationAccessAsync(shipmentId, cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(new CancelShipmentCommand(shipmentId), cancellationToken);

        return result.IsFailure ? this.ToProblem(result.Error) : NoContent();
    }

    [HttpGet("{shipmentId:guid}")]
    public async Task<IActionResult> GetById(Guid shipmentId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetShipmentQuery(shipmentId), cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        var tenantWide = await accessService.HasTenantWideReadAsync(cancellationToken);
        var assigned = await accessService.GetAssignedWarehouseIdsAsync(cancellationToken);
        var visibleDeliveries = (result.Value.Deliveries ?? Array.Empty<ShipmentDeliveryResponse>())
            .Where(x => tenantWide || (assigned.Contains(x.WarehouseId) && x.Status is "DeliveredToWarehouse" or "Received"))
            .ToArray();
        if (!tenantWide && visibleDeliveries.Length == 0)
            return Forbid();

        if (await accessService.HasPermissionAsync(TenantPermission.ViewCosts, cancellationToken))
            return Ok(result.Value);

        var shipment = result.Value;
        // Explicit operational projection: never serialize original costs or
        // unclassified document metadata to operational roles.
        return Ok(new
        {
            shipment.Id, shipment.SupplierId, shipment.TrackingNumber,
            shipment.OriginCountry, shipment.Status, shipment.ExpectedArrivalAtUtc,
            shipment.ShippedAtUtc, shipment.ArrivedAtUtc, shipment.ReceivedAtUtc,
            shipment.CancelledAtUtc, shipment.CreatedAtUtc, shipment.UpdatedAtUtc,
            Lines = shipment.Lines.Select(line => new
            {
                line.Id, line.ProductId, line.ProductVarietyId, line.BatchCode,
                line.ExpectedQuantity, line.ReceivedQuantity, line.PackSizeKg, line.FruitsPerPack
            }),
            shipment.ContainerNumber, shipment.BillOfLadingNumber,
            shipment.PortOfLoading, shipment.PortOfDischarge, shipment.VesselName,
            shipment.VoyageNumber, shipment.CustomsBrokerName, shipment.BillOfEntryNumber,
            shipment.BillOfEntryDate, shipment.OutOfChargeAtUtc, shipment.CustomsStatus,
            shipment.PlantQuarantineStatus, shipment.FoodSafetyStatus,
            shipment.ClearanceEvents, Documents = Array.Empty<object>(),
            Receipts = (shipment.Receipts ?? Array.Empty<ShipmentReceiptResponse>()).Where(x => tenantWide || assigned.Contains(x.WarehouseId)),
            Deliveries = visibleDeliveries, shipment.IsClearedForWarehouseDispatch
        });
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? searchTerm,
        [FromQuery] ShipmentStatus? status,
        [FromQuery] Guid? supplierId,
        CancellationToken cancellationToken)
    {
        if (!await accessService.HasTenantAccessAsync(cancellationToken))
        {
            return Forbid();
        }

        var result = await sender.Send(new SearchShipmentsQuery(searchTerm, status, supplierId), cancellationToken);

        if (await accessService.HasTenantWideReadAsync(cancellationToken))
            return Ok(result);

        var assigned = await accessService.GetAssignedWarehouseIdsAsync(cancellationToken);
        var visible = new List<CrateSync.Platform.InboundLogistics.Application.Shipments.SearchShipments.ShipmentSummaryResponse>();
        foreach (var summary in result)
        {
            var shipment = await sender.Send(new GetShipmentQuery(summary.Id), cancellationToken);
            if (!shipment.IsFailure && shipment.Value.Deliveries?.Any(x =>
                assigned.Contains(x.WarehouseId) && x.Status is "DeliveredToWarehouse" or "Received") == true)
                visible.Add(summary);
        }
        return Ok(visible);
    }

    private async Task<IActionResult?> EnsureShipmentOperationAccessAsync(
        Guid shipmentId,
        CancellationToken cancellationToken)
    {
        var shipment = await sender.Send(new GetShipmentQuery(shipmentId), cancellationToken);

        if (shipment.IsFailure)
            return this.ToProblem(shipment.Error);

        return await accessService.IsTenantAdminAsync(cancellationToken) ? null : Forbid();
    }
}
