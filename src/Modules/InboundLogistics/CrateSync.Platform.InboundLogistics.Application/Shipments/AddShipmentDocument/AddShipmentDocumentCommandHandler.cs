using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.Common;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.AddShipmentDocument;

internal sealed class AddShipmentDocumentCommandHandler(
    IInboundShipmentRepository repository,
    IInboundLogisticsUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
    : ICommandHandler<AddShipmentDocumentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddShipmentDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(
            InboundShipmentId.From(request.ShipmentId),
            tenantContext.TenantId,
            cancellationToken);

        if (shipment is null)
            return Result<Guid>.Failure(
                ShipmentErrors.NotFound(request.ShipmentId));

        var documentId = shipment.AddDocument(
            request.Type,
            request.DocumentName,
            request.ReferenceNumber,
            request.Notes,
            timeProvider.GetUtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(documentId.Value);
    }
}
