using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.SearchShipments;

internal sealed class SearchShipmentsQueryHandler(
    IInboundLogisticsReadService readService,
    ITenantContext tenantContext) :
    IQueryHandler<SearchShipmentsQuery, IReadOnlyCollection<ShipmentSummaryResponse>>
{
    private readonly IInboundLogisticsReadService _readService = readService;
    private readonly ITenantContext _tenantContext = tenantContext;

    public Task<IReadOnlyCollection<ShipmentSummaryResponse>> Handle(
        SearchShipmentsQuery request,
        CancellationToken cancellationToken)
    {
        return _readService.SearchAsync(
            _tenantContext.TenantId,
            request.SearchTerm,
            request.Status,
            request.SupplierId,
            request.WarehouseId,
            cancellationToken);
    }
}
