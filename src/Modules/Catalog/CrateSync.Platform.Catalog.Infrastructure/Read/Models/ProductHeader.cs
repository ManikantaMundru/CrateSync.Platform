namespace CrateSync.Platform.Catalog.Infrastructure.Read.Models;
internal sealed record ProductHeader(
    Guid Id,
    string Name,
    bool IsActive);
