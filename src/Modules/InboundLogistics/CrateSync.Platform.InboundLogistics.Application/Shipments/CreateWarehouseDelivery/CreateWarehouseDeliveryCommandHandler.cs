using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.Common;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.CreateWarehouseDelivery;

internal sealed class CreateWarehouseDeliveryCommandHandler(
    IInboundShipmentRepository repository,
    IInboundLogisticsUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
    : ICommandHandler<CreateWarehouseDeliveryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateWarehouseDeliveryCommand request,
        CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(
            InboundShipmentId.From(request.ShipmentId),
            tenantContext.TenantId,
            cancellationToken);

        if (shipment is null)
            return Result.Failure<Guid>(ShipmentErrors.NotFound(request.ShipmentId));

        try
        {
            var deliveryId = shipment.CreateWarehouseDelivery(
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
                    .Select(x => (
                        ShipmentLineId.From(x.ShipmentLineId),
                        x.Quantity))
                    .ToArray(),
                timeProvider.GetUtcNow());

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(deliveryId.Value);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<Guid>(
                ShipmentErrors.Conflict(
                    "InboundLogistics.Shipment.InvalidWarehouseDelivery",
                    ex.Message));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(
                ShipmentErrors.Conflict(
                    "InboundLogistics.Shipment.InvalidWarehouseDelivery",
                    ex.Message));
        }
    }
}
