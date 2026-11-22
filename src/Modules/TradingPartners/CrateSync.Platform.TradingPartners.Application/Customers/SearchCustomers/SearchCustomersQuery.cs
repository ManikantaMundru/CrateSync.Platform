using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.TradingPartners.Application.Common;

namespace CrateSync.Platform.TradingPartners.Application.Customers.SearchCustomers;

public sealed record SearchCustomersQuery(
    string? SearchTerm,
    bool? IsActive,
    int Page = 1,
    int PageSize = 25)
    : IQuery<PagedResponse<CustomerSummaryResponse>>;
