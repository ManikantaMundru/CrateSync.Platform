namespace CrateSync.Platform.Api.Requests.Finance;

public sealed record CreateSupplierInvoiceRequest(
    Guid SupplierId,
    Guid ShipmentId,
    string InvoiceNumber,
    string Currency,
    DateTimeOffset InvoiceDateUtc,
    IReadOnlyCollection<CreateSupplierInvoiceLineRequest> Lines);

public sealed record CreateSupplierInvoiceLineRequest(
    string Description,
    Guid? ProductVarietyId,
    int Quantity,
    decimal UnitCost);
