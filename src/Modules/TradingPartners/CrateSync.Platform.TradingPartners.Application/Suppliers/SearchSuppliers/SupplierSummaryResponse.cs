namespace CrateSync.Platform.TradingPartners.Application.Suppliers.SearchSuppliers;

public sealed record SupplierSummaryResponse(
    Guid Id,
    string Name,
    string? CommercialIdentifier,
    bool IsActive);
