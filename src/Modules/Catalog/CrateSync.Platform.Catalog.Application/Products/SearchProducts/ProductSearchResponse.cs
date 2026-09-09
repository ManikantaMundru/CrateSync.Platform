namespace CrateSync.Platform.Catalog.Application.Products.SearchProducts;

public sealed record ProductSearchResponse(
    Guid Id,
    string Name,
    bool IsActive,
    int VarietyCount);
