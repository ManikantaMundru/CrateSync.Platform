namespace CrateSync.Platform.Warehousing.Contracts;

public sealed record WarehouseContract(
    Guid WarehouseId,
    Guid TenantId,
    string Name,
    string Code,
    bool IsActive);
