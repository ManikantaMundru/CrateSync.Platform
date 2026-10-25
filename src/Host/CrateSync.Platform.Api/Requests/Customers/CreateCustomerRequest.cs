namespace CrateSync.Platform.Api.Requests.Customers;
public sealed record CreateCustomerRequest(
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
