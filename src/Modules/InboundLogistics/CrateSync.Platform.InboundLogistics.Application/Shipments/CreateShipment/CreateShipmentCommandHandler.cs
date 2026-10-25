using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.Common;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using CrateSync.Platform.InboundLogistics.Domain.ValueObjects;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.CreateShipment;

internal sealed class CreateShipmentCommandHandler(
    IInboundShipmentRepository repository,
    IInboundLogisticsUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider) :
    ICommandHandler<CreateShipmentCommand, Result<Guid>>
{
    private readonly IInboundShipmentRepository _repository = repository;
    private readonly IInboundLogisticsUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result<Guid>> Handle(
        CreateShipmentCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var trackingNumber = request.TrackingNumber.Trim();

        if (await _repository.TrackingNumberExistsAsync(
                tenantId,
                trackingNumber,
                cancellationToken))
        {
            return Result<Guid>.Failure(
                ShipmentErrors.TrackingNumberAlreadyExists(trackingNumber));
        }

        var shipment = InboundShipment.Create(
            tenantId,
            request.SupplierId,
            request.WarehouseId,
            new TrackingNumber(trackingNumber),
            new CountryCode(request.OriginCountry),
            request.ExpectedArrivalAtUtc,
            _timeProvider.GetUtcNow());

        _repository.Add(shipment);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(shipment.Id.Value);
    }
}
