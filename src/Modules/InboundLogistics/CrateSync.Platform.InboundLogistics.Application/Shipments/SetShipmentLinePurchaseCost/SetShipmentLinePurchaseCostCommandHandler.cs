using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.Common;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using CrateSync.Platform.InboundLogistics.Domain.ValueObjects;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.SetShipmentLinePurchaseCost;

internal sealed class SetShipmentLinePurchaseCostCommandHandler(
    IInboundShipmentRepository repository, IInboundLogisticsUnitOfWork unitOfWork,
    ITenantContext tenantContext, TimeProvider timeProvider)
    : ICommandHandler<SetShipmentLinePurchaseCostCommand, Result>
{
    public async Task<Result> Handle(SetShipmentLinePurchaseCostCommand request, CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(InboundShipmentId.From(request.ShipmentId), tenantContext.TenantId, cancellationToken);
        if (shipment is null)
            return Result.Failure(ShipmentErrors.NotFound(request.ShipmentId));
        try
        {
            shipment.SetLinePurchaseCost(ShipmentLineId.From(request.LineId),
                new Money(request.UnitCost, new Currency(request.Currency)), timeProvider.GetUtcNow());
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ShipmentErrors.Conflict("InboundLogistics.Shipment.InvalidPurchaseCost", ex.Message));
        }
    }
}
