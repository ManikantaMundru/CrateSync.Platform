using CrateSync.Platform.Finance.Domain.Common;

namespace CrateSync.Platform.Finance.Application.Receivables.SearchCustomerInvoices;

public sealed record CustomerInvoiceSummaryResponse(
    Guid Id,
    Guid CustomerId,
    Guid SalesOrderId,
    string InvoiceNumber,
    string Currency,
    InvoiceStatus Status,
    decimal TotalAmount,
    decimal AmountPaid,
    decimal OutstandingAmount,
    DateTimeOffset InvoiceDateUtc);
