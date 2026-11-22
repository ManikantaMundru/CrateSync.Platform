namespace CrateSync.Platform.TradingPartners.Application.Suppliers.SearchSuppliers;

public sealed record SupplierSummaryResponse(
    Guid Id,
    string Name,
    string? CommercialIdentifier,
    bool IsActive,
    string? AddressLine1,
    string? City,
    string? StateOrProvince,
    string? Country,
    string? ContactName,
    string? Email,
    string? Phone);
