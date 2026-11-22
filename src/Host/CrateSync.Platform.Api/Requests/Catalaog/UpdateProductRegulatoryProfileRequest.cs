namespace CrateSync.Platform.Api.Requests.Catalaog;

public sealed record UpdateProductRegulatoryProfileRequest(
    string? CustomsTariffCode,
    string? ScientificName,
    string? RegulatoryCategory,
    string? DefaultUnitOfMeasure);
