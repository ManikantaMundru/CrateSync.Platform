using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Catalog.Application.Products.ActivateVariety;

public sealed record ActivateProductVarietyCommand(
    Guid ProductId,
    Guid VarietyId)
    : ICommand<Result>;
