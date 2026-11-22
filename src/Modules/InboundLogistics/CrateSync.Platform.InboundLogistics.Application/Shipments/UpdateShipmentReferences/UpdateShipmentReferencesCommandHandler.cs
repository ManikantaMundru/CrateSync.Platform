using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.Common;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.UpdateShipmentReferences;

internal sealed class UpdateShipmentReferencesCommandHandler(
    IInboundShipmentRepository repository,
    IInboundLogisticsUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateShipmentReferencesCommand, Result>
{
    public async Task<Result> Handle(
        UpdateShipmentReferencesCommand request,
        CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(
            InboundShipmentId.From(request.ShipmentId),
            tenantContext.TenantId,
            cancellationToken);

        if (shipment is null)
            return Result.Failure(
                ShipmentErrors.NotFound(request.ShipmentId));

        shipment.UpdateShippingAndCustomsReferences(
            request.ContainerNumber,
            request.BillOfLadingNumber,
            request.PortOfLoading,
            request.PortOfDischarge,
            request.VesselName,
            request.VoyageNumber,
            request.CustomsBrokerName,
            request.BillOfEntryNumber,
            request.BillOfEntryDate,
            request.OutOfChargeAtUtc,
            timeProvider.GetUtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
