using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.Common;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using CrateSync.Platform.InboundLogistics.Domain.ValueObjects;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.AddShipmentLine;

internal sealed class AddShipmentLineCommandHandler(
    IInboundShipmentRepository repository,
    IInboundLogisticsUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider) :
    ICommandHandler<AddShipmentLineCommand, Result<Guid>>
{
    private readonly IInboundShipmentRepository _repository = repository;
    private readonly IInboundLogisticsUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result<Guid>> Handle(
        AddShipmentLineCommand request,
        CancellationToken cancellationToken)
    {
        var shipment = await _repository.GetByIdAsync(
            InboundShipmentId.From(request.ShipmentId),
            _tenantContext.TenantId,
            cancellationToken);

        if (shipment is null)
            return Result<Guid>.Failure(ShipmentErrors.NotFound(request.ShipmentId));

        var lineId = shipment.AddLine(
            request.ProductId,
            request.ProductVarietyId,
            new BatchCode(request.BatchCode),
            new CrateQuantity(request.ExpectedQuantity),
            new PackSize(request.PackSizeKg),
            new Money(
                request.UnitCost,
                new Currency(request.Currency)),
            _timeProvider.GetUtcNow());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(lineId.Value);
    }
}
