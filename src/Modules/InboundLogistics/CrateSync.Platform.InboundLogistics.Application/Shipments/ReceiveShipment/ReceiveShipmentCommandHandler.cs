using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.Common;
using CrateSync.Platform.InboundLogistics.Contracts;
using MediatR;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using CrateSync.Platform.InboundLogistics.Domain.ValueObjects;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.ReceiveShipment;

internal sealed class ReceiveShipmentCommandHandler(
    IInboundShipmentRepository repository,
    IInboundLogisticsUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider,
    IPublisher publisher) :
    ICommandHandler<ReceiveShipmentCommand, Result>
{
    private readonly IInboundShipmentRepository _repository = repository;
    private readonly IInboundLogisticsUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result> Handle(
        ReceiveShipmentCommand request,
        CancellationToken cancellationToken)
    {
        var shipment = await _repository.GetByIdAsync(
            InboundShipmentId.From(request.ShipmentId),
            _tenantContext.TenantId,
            cancellationToken);

        if (shipment is null)
            return Result.Failure(ShipmentErrors.NotFound(request.ShipmentId));

        var occurredAtUtc =
            _timeProvider.GetUtcNow();

        var receiptId = shipment.ReceiveLine(
            ShipmentReceiptId.From(request.ReceiptId),
            ShipmentLineId.From(request.ShipmentLineId),
            new CrateQuantity(request.Quantity),
            occurredAtUtc);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var line = shipment.Lines.Single(
            x => x.Id == ShipmentLineId.From(
                request.ShipmentLineId));

        await _publisher.Publish(
            new ShipmentLineReceivedIntegrationEvent(
                receiptId.Value,
                shipment.Id.Value,
                shipment.TenantId,
                shipment.WarehouseId,
                line.ProductVarietyId,
                request.Quantity,
                occurredAtUtc),
            cancellationToken);

        return Result.Success();
    }
}
