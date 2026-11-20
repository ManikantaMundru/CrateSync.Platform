using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Finance.Domain.Common;

namespace CrateSync.Platform.Finance.Domain.Payables.Events;

public sealed record SupplierInvoiceCreatedDomainEvent(
    SupplierInvoiceId SupplierInvoiceId,
    Guid TenantId,
    Guid SupplierId,
    Guid ShipmentId,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset OccurredAtUtc) : DomainEvent(OccurredAtUtc);
