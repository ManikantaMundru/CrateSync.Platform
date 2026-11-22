using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Catalog.Application.Products.DeactivateVariety;

public sealed record DeactivateProductVarietyCommand(
    Guid ProductId,
    Guid VarietyId)
    : ICommand<Result>;
