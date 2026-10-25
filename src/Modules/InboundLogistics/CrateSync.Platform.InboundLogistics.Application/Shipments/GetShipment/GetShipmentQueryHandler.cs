using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.Common;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.GetShipment;

internal sealed class GetShipmentQueryHandler(
    IInboundLogisticsReadService readService,
    ITenantContext tenantContext) :
    IQueryHandler<GetShipmentQuery, Result<ShipmentResponse>>
{
    private readonly IInboundLogisticsReadService _readService = readService;
    private readonly ITenantContext _tenantContext = tenantContext;

    public async Task<Result<ShipmentResponse>> Handle(
        GetShipmentQuery request,
        CancellationToken cancellationToken)
    {
        var shipment = await _readService.GetByIdAsync(
            _tenantContext.TenantId,
            request.ShipmentId,
            cancellationToken);

        return shipment is null
            ? Result<ShipmentResponse>.Failure(
                ShipmentErrors.NotFound(request.ShipmentId))
            : Result<ShipmentResponse>.Success(shipment);
    }
}
