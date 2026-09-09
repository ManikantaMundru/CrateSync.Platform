namespace CrateSync.Platform.Catalog.Infrastructure.Read.Models;

internal sealed record ProductRow(
    Guid Id,
    string Name,
    bool IsActive);
