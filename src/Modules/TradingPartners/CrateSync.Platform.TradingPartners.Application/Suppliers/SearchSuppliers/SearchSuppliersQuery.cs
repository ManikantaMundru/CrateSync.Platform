using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.TradingPartners.Application.Common;

namespace CrateSync.Platform.TradingPartners.Application.Suppliers.SearchSuppliers;

public sealed record SearchSuppliersQuery(
    string? SearchTerm,
    bool? IsActive,
    int Page = 1,
    int PageSize = 25)
    : IQuery<PagedResponse<SupplierSummaryResponse>>;
