using CrateSync.Platform.InboundLogistics.Application.Shipments.GetShipment;
using CrateSync.Platform.InboundLogistics.Application.Shipments.SearchShipments;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;

namespace CrateSync.Platform.InboundLogistics.Application.Abstractions;

public interface IInboundLogisticsReadService
{
    Task<ShipmentResponse?> GetByIdAsync(
        Guid tenantId,
        Guid shipmentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ShipmentSummaryResponse>> SearchAsync(
        Guid tenantId,
        string? searchTerm,
        ShipmentStatus? status,
        Guid? supplierId,
        Guid? warehouseId,
        CancellationToken cancellationToken = default);
}
