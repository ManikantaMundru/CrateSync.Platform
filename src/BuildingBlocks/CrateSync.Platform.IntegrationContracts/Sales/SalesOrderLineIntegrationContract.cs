namespace CrateSync.Platform.IntegrationContracts.Sales;

public sealed record SalesOrderLineIntegrationContract(
    Guid SalesOrderLineId,
    Guid ProductVarietyId,
    int Quantity,
    string OriginCountry = "ZZ");
