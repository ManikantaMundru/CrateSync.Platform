using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Finance.Domain.Common;

namespace CrateSync.Platform.Finance.Domain.Receivables.Events;

public sealed record CustomerInvoiceCreatedDomainEvent(
    CustomerInvoiceId CustomerInvoiceId,
    Guid TenantId,
    Guid CustomerId,
    Guid SalesOrderId,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset OccurredAtUtc) : DomainEvent(OccurredAtUtc);
