namespace CrateSync.Platform.Api.Requests.Sales;

public sealed record AddSalesOrderLineRequest(
    Guid ProductVarietyId,
    int Quantity,
    decimal PackSize,
    decimal UnitPrice,
    string Currency,
    string OriginCountry = "ZZ");
