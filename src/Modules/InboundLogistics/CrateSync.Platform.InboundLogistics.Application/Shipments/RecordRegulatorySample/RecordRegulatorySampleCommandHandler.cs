using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.Common;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.RecordRegulatorySample;
internal sealed class RecordRegulatorySampleCommandHandler(IInboundShipmentRepository repository, IInboundLogisticsUnitOfWork unitOfWork, ITenantContext tenantContext, TimeProvider timeProvider) : ICommandHandler<RecordRegulatorySampleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RecordRegulatorySampleCommand request, CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(InboundShipmentId.From(request.ShipmentId), tenantContext.TenantId, cancellationToken);
        if (shipment is null) return Result<Guid>.Failure(ShipmentErrors.NotFound(request.ShipmentId));
        if (request.LineId == Guid.Empty) return Result<Guid>.Failure(ShipmentErrors.Conflict("InboundLogistics.Sample.Invalid", "Shipment line is required."));
        try
        {
            var id = shipment.RecordRegulatorySample(request.SampleId, ShipmentLineId.From(request.LineId), request.Authority, request.Quantity, request.ReferenceNumber, request.Remarks, timeProvider.GetUtcNow());
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(id);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure(ShipmentErrors.Conflict("InboundLogistics.Sample.Invalid", ex.Message));
        }
    }
}
