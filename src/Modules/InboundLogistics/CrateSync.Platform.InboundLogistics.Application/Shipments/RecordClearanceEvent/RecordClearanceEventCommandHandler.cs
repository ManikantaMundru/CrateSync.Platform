using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.Common;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.RecordClearanceEvent;

internal sealed class RecordClearanceEventCommandHandler(
    IInboundShipmentRepository repository,
    IInboundLogisticsUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
    : ICommandHandler<RecordClearanceEventCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        RecordClearanceEventCommand request,
        CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(
            InboundShipmentId.From(request.ShipmentId),
            tenantContext.TenantId,
            cancellationToken);

        if (shipment is null)
            return Result<Guid>.Failure(
                ShipmentErrors.NotFound(request.ShipmentId));

        var eventId = shipment.RecordClearanceEvent(
            request.Authority,
            request.Status,
            request.ReferenceNumber,
            request.Remarks,
            timeProvider.GetUtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(eventId.Value);
    }
}
