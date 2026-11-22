namespace CrateSync.Platform.Catalog.Infrastructure.Read.Models;
internal sealed record ProductHeader(
    Guid Id,
    string Name,
    bool IsActive,
    string? CustomsTariffCode,
    string? ScientificName,
    string? RegulatoryCategory,
    string? DefaultUnitOfMeasure);
