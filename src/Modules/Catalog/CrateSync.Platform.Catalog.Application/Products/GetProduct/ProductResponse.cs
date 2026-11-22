namespace CrateSync.Platform.Catalog.Application.Products.GetProduct;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    bool IsActive,
    string? CustomsTariffCode,
    string? ScientificName,
    string? RegulatoryCategory,
    string? DefaultUnitOfMeasure,
    IReadOnlyCollection<ProductVarietyResponse> Varieties);

public sealed record ProductVarietyResponse(
    Guid Id,
    string Name,
    bool IsActive);
