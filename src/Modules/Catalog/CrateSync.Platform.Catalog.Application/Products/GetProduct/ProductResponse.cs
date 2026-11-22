namespace CrateSync.Platform.Catalog.Application.Products.GetProduct;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    bool IsActive,
    IReadOnlyCollection<ProductVarietyResponse> Varieties,
    string? CustomsTariffCode = null,
    string? ScientificName = null,
    string? RegulatoryCategory = null,
    string? DefaultUnitOfMeasure = null);

public sealed record ProductVarietyResponse(
    Guid Id,
    string Name,
    bool IsActive);
