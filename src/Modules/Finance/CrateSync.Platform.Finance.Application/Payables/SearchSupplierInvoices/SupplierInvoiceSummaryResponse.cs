using CrateSync.Platform.Finance.Domain.Common;

namespace CrateSync.Platform.Finance.Application.Payables.SearchSupplierInvoices;

public sealed record SupplierInvoiceSummaryResponse(
    Guid Id,
    Guid SupplierId,
    Guid ShipmentId,
    string InvoiceNumber,
    string Currency,
    InvoiceStatus Status,
    decimal TotalAmount,
    decimal AmountPaid,
    decimal OutstandingAmount,
    DateTimeOffset InvoiceDateUtc);
