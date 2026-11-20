using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Finance.Domain.Common;

namespace CrateSync.Platform.Finance.Domain.Receivables.Events;

public sealed record CustomerInvoicePaidDomainEvent(
    CustomerInvoiceId CustomerInvoiceId,
    Guid TenantId,
    DateTimeOffset OccurredAtUtc) : DomainEvent(OccurredAtUtc);
