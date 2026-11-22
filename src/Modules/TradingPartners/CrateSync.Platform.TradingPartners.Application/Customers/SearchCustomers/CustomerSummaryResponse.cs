namespace CrateSync.Platform.TradingPartners.Application.Customers.SearchCustomers;

public sealed record CustomerSummaryResponse(
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
