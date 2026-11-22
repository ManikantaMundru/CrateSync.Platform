using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Catalog.Application.Products.UpdateRegulatoryProfile;

public sealed record UpdateProductRegulatoryProfileCommand(
    Guid ProductId,
    string? CustomsTariffCode,
    string? ScientificName,
    string? RegulatoryCategory,
    string? DefaultUnitOfMeasure)
    : ICommand<Result>;
