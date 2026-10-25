using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.TradingPartners.Domain.Suppliers.Events;

public sealed record SupplierDeactivatedDomainEvent(
    SupplierId SupplierId,
    Guid TenantId,
    DateTimeOffset OccurredOnUtc)
    : DomainEvent(OccurredOnUtc);
