using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.Common;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using CrateSync.Platform.InboundLogistics.Domain.ValueObjects;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.ReceiveShipment;

internal sealed class ReceiveShipmentCommandHandler(
    IInboundShipmentRepository repository,
    IInboundLogisticsUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider) :
    ICommandHandler<ReceiveShipmentCommand, Result>
{
    private readonly IInboundShipmentRepository _repository = repository;
    private readonly IInboundLogisticsUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly TimeProvider _timeProvider = timeProvider;

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

        shipment.ReceiveLine(
            ShipmentLineId.From(request.ShipmentLineId),
            new CrateQuantity(request.Quantity),
            _timeProvider.GetUtcNow());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
