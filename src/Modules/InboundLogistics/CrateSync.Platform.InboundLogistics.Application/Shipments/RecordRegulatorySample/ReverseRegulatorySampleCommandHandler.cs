using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.Common;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
namespace CrateSync.Platform.InboundLogistics.Application.Shipments.RecordRegulatorySample;
internal sealed class ReverseRegulatorySampleCommandHandler(IInboundShipmentRepository repository, IInboundLogisticsUnitOfWork unitOfWork, ITenantContext tenantContext, TimeProvider timeProvider) : ICommandHandler<ReverseRegulatorySampleCommand, Result>
{
    public async Task<Result> Handle(ReverseRegulatorySampleCommand request, CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(InboundShipmentId.From(request.ShipmentId), tenantContext.TenantId, cancellationToken);
        if (shipment is null) return Result.Failure(ShipmentErrors.NotFound(request.ShipmentId));
        try
        {
            shipment.ReverseRegulatorySample(request.SampleId, request.Reason, timeProvider.GetUtcNow());
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ShipmentErrors.Conflict("InboundLogistics.Sample.InvalidReversal", ex.Message));
        }
    }
}
