using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Finance.Application.Common;

internal static class FinanceErrors
{
    public static readonly Error CustomerInvoiceNotFound = new(
        "Finance.CustomerInvoiceNotFound",
        "Customer invoice was not found.",
        ErrorType.NotFound);

    public static readonly Error SupplierInvoiceNotFound = new(
        "Finance.SupplierInvoiceNotFound",
        "Supplier invoice was not found.",
        ErrorType.NotFound);

    public static readonly Error InvoiceNumberExists = new(
        "Finance.InvoiceNumberExists",
        "An invoice with this invoice number already exists.",
        ErrorType.Conflict);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);
}
