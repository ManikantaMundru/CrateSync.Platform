using CrateSync.Platform.InboundLogistics.Domain.Shipments;

namespace CrateSync.Platform.InboundLogistics.Application.Abstractions;

public interface IInboundShipmentRepository
{
    Task<InboundShipment?> GetByIdAsync(
        InboundShipmentId shipmentId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<bool> TrackingNumberExistsAsync(
        Guid tenantId,
        string trackingNumber,
        CancellationToken cancellationToken = default);

    void Add(InboundShipment shipment);
}
