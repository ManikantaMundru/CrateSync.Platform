namespace CrateSync.Platform.TradingPartners.Application.Customers.GetCustomer;

public sealed record CustomerResponse(
    Guid Id,
    string Name,
    string? CommercialIdentifier,
    bool IsActive,
    AddressResponse? Address,
    ContactDetailsResponse? ContactDetails);

public sealed record AddressResponse(
    string Line1,
    string? Line2,
    string City,
    string? StateOrProvince,
    string PostalCode,
    string Country);

public sealed record ContactDetailsResponse(
    string? ContactName,
    string? Email,
    string? Phone);
