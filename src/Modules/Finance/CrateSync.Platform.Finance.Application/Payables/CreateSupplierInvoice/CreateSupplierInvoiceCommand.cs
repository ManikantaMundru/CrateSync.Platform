using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Finance.Application.Payables.CreateSupplierInvoice;

public sealed record CreateSupplierInvoiceCommand(
    Guid SupplierId,
    Guid ShipmentId,
    string InvoiceNumber,
    string Currency,
    DateTimeOffset InvoiceDateUtc,
    IReadOnlyCollection<CreateSupplierInvoiceLine> Lines)
    : ICommand<Result<Guid>>;

public sealed record CreateSupplierInvoiceLine(
    string Description,
    Guid? ProductVarietyId,
    int Quantity,
    decimal UnitCost);
