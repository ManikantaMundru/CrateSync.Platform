using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.Catalog.Application.Products.SearchProducts;

public sealed record SearchProductsQuery(
    string? SearchTerm,
    bool? IsActive)
    : IQuery<IReadOnlyCollection<ProductSearchResponse>>;
