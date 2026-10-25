namespace CrateSync.Platform.TradingPartners.Application.Suppliers.GetSupplier;

public sealed record SupplierResponse(
    Guid Id,
    string Name,
    string? CommercialIdentifier,
    bool IsActive,
    SupplierAddressResponse? Address,
    SupplierContactDetailsResponse? ContactDetails);

public sealed record SupplierAddressResponse(
    string Line1,
    string? Line2,
    string City,
    string? StateOrProvince,
    string PostalCode,
    string Country);

public sealed record SupplierContactDetailsResponse(
    string? ContactName,
    string? Email,
    string? Phone);
