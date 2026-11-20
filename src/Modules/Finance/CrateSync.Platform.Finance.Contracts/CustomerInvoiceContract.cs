namespace CrateSync.Platform.Finance.Contracts;

public sealed record CustomerInvoiceContract(
    Guid CustomerInvoiceId,
    Guid TenantId,
    Guid CustomerId,
    Guid SalesOrderId,
    string InvoiceNumber,
    string Currency,
    string Status,
    decimal TotalAmount,
    decimal AmountPaid,
    decimal OutstandingAmount);
