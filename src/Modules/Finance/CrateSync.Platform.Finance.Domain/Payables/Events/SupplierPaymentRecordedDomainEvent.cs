using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Finance.Domain.Common;

namespace CrateSync.Platform.Finance.Domain.Payables.Events;

public sealed record SupplierPaymentRecordedDomainEvent(
    SupplierInvoiceId SupplierInvoiceId,
    SupplierPaymentId SupplierPaymentId,
    Guid TenantId,
    decimal Amount,
    string Currency,
    DateTimeOffset OccurredAtUtc) : DomainEvent(OccurredAtUtc);
