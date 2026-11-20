namespace CrateSync.Platform.Finance.Contracts;

public sealed record SupplierInvoiceContract(
    Guid SupplierInvoiceId,
    Guid TenantId,
    Guid SupplierId,
    Guid ShipmentId,
    string InvoiceNumber,
    string Currency,
    string Status,
    decimal TotalAmount,
    decimal AmountPaid,
    decimal OutstandingAmount);
