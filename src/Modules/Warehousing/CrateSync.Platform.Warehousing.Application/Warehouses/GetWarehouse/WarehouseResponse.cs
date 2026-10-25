namespace CrateSync.Platform.Warehousing.Application.Warehouses.GetWarehouse;

public sealed record WarehouseResponse(
    Guid Id,
    string Name,
    string Code,
    bool IsActive,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? StateOrProvince,
    string PostalCode,
    string Country,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeactivatedAtUtc);
