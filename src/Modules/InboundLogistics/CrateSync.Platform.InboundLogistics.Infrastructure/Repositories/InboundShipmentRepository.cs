using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using CrateSync.Platform.InboundLogistics.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Repositories;

internal sealed class InboundShipmentRepository(InboundLogisticsDbContext dbContext) : IInboundShipmentRepository
{
    private readonly InboundLogisticsDbContext _dbContext = dbContext;

    public Task<InboundShipment?> GetByIdAsync(
        InboundShipmentId shipmentId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.InboundShipments
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(
                x => x.Id == shipmentId && x.TenantId == tenantId,
                cancellationToken);
    }

    public Task<bool> TrackingNumberExistsAsync(
        Guid tenantId,
        string trackingNumber,
        CancellationToken cancellationToken = default)
    {
        var normalized = trackingNumber.Trim();

        return _dbContext.InboundShipments
            .AnyAsync(
                x => x.TenantId == tenantId &&
                     x.TrackingNumber == new TrackingNumber(normalized),
                cancellationToken);
    }

    public void Add(InboundShipment shipment)
    {
        _dbContext.InboundShipments.Add(shipment);
    }
}
