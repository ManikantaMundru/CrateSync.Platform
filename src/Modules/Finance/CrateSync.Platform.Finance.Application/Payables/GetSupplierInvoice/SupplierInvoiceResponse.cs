namespace CrateSync.Platform.Finance.Application.Payables.GetSupplierInvoice;

public sealed record SupplierInvoiceResponse(
    Guid Id,
    Guid SupplierId,
    Guid ShipmentId,
    string InvoiceNumber,
    string Currency,
    string Status,
    decimal TotalAmount,
    decimal AmountPaid,
    decimal OutstandingAmount,
    DateTimeOffset InvoiceDateUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyCollection<SupplierInvoiceLineResponse> Lines,
    IReadOnlyCollection<SupplierPaymentResponse> Payments);

public sealed record SupplierInvoiceLineResponse(
    Guid Id,
    string Description,
    Guid? ProductVarietyId,
    int Quantity,
    decimal UnitCost,
    decimal LineTotal);

public sealed record SupplierPaymentResponse(
    Guid Id,
    decimal Amount,
    string Reference,
    DateTimeOffset PaidAtUtc);
