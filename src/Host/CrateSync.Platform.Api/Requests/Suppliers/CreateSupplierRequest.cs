namespace CrateSync.Platform.Api.Requests.Suppliers;

public sealed record CreateSupplierRequest(
    string Name,
    string? CommercialIdentifier,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? StateOrProvince,
    string? PostalCode,
    string? Country,
    string? ContactName,
    string? Email,
    string? Phone);
