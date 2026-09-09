namespace CrateSync.Platform.Catalog.Infrastructure.Read.Models;

internal sealed record ProductVarietyRow(
    Guid Id,
    string Name,
    bool IsActive);
