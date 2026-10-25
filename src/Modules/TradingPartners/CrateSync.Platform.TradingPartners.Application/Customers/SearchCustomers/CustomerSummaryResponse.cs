namespace CrateSync.Platform.TradingPartners.Application.Customers.SearchCustomers;

public sealed record CustomerSummaryResponse(
    Guid Id,
    string Name,
    string? CommercialIdentifier,
    bool IsActive);
