using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.Common;
using CrateSync.Platform.InboundLogistics.Contracts;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using MediatR;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.ConfirmWarehouseDelivery;

internal sealed class ConfirmWarehouseDeliveryCommandHandler(
    IInboundShipmentRepository repository,
    IInboundLogisticsUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider,
    IPublisher publisher)
    : ICommandHandler<ConfirmWarehouseDeliveryCommand, Result>
{
    public async Task<Result> Handle(
        ConfirmWarehouseDeliveryCommand request,
        CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(
            InboundShipmentId.From(request.ShipmentId),
            tenantContext.TenantId,
            cancellationToken);

        if (shipment is null)
            return Result.Failure(ShipmentErrors.NotFound(request.ShipmentId));

        try
        {
            var occurredAtUtc = timeProvider.GetUtcNow();
            var receipts = shipment.ConfirmWarehouseDelivery(
                ShipmentDeliveryId.From(request.DeliveryId),
                occurredAtUtc);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var receipt in receipts)
            {
                await publisher.Publish(
                    new ShipmentLineReceivedIntegrationEvent(
                        receipt.Id.Value,
                        shipment.Id.Value,
                        shipment.TenantId,
                        receipt.WarehouseId,
                        receipt.ProductVarietyId,
                        shipment.OriginCountry.Value,
                        receipt.Quantity,
                        receipt.ReceivedAtUtc),
                    cancellationToken);
            }

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(
                ShipmentErrors.Conflict(
                    "InboundLogistics.Shipment.InvalidWarehouseDelivery",
                    ex.Message));
        }
    }
}
