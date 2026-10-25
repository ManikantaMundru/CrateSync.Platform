using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.TradingPartners.Application.Suppliers.SearchSuppliers;

public sealed record SearchSuppliersQuery(
    string? SearchTerm,
    bool? IsActive)
    : IQuery<IReadOnlyCollection<SupplierSummaryResponse>>;
