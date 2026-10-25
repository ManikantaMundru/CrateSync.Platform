using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.TradingPartners.Application.Customers.SearchCustomers;

public sealed record SearchCustomersQuery(
    string? SearchTerm,
    bool? IsActive)
    : IQuery<IReadOnlyCollection<CustomerSummaryResponse>>;
