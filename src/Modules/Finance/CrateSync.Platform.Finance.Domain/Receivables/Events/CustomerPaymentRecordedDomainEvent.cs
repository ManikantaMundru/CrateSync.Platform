using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Finance.Domain.Common;

namespace CrateSync.Platform.Finance.Domain.Receivables.Events;

public sealed record CustomerPaymentRecordedDomainEvent(
    CustomerInvoiceId CustomerInvoiceId,
    CustomerPaymentId CustomerPaymentId,
    Guid TenantId,
    decimal Amount,
    string Currency,
    DateTimeOffset OccurredAtUtc) : DomainEvent(OccurredAtUtc);
