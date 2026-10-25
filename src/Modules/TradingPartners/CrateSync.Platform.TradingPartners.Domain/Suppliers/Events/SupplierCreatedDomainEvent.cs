using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.TradingPartners.Domain.Suppliers.Events;

public sealed record SupplierCreatedDomainEvent(
    SupplierId SupplierId,
    Guid TenantId,
    string Name,
    DateTimeOffset OccurredOnUtc)
    : DomainEvent(OccurredOnUtc);
