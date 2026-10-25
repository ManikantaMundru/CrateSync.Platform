namespace CrateSync.Platform.Api.Requests.Warehouse;

public sealed record CreateWarehouseRequest(
    string Name,
    string Code,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? StateOrProvince,
    string PostalCode,
    string Country);
