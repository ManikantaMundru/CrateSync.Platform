namespace CrateSync.Platform.Finance.Application.Receivables.GetCustomerInvoice;

public sealed record CustomerInvoiceResponse(
    Guid Id,
    Guid CustomerId,
    Guid SalesOrderId,
    string InvoiceNumber,
    string Currency,
    string Status,
    decimal TotalAmount,
    decimal AmountPaid,
    decimal OutstandingAmount,
    DateTimeOffset InvoiceDateUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyCollection<CustomerInvoiceLineResponse> Lines,
    IReadOnlyCollection<CustomerPaymentResponse> Payments);

public sealed record CustomerInvoiceLineResponse(
    Guid Id,
    Guid ProductVarietyId,
    int Quantity,
    decimal PackSize,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record CustomerPaymentResponse(
    Guid Id,
    decimal Amount,
    string Reference,
    DateTimeOffset PaidAtUtc);
