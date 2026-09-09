namespace CrateSync.Platform.Catalog.Application.Products.GetProduct;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    bool IsActive,
    IReadOnlyCollection<ProductVarietyResponse> Varieties);

public sealed record ProductVarietyResponse(
    Guid Id,
    string Name,
    bool IsActive);
