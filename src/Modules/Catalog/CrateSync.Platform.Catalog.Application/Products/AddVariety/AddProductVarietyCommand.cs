using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Catalog.Application.Products.AddVariety;

public sealed record AddProductVarietyCommand(
    Guid ProductId,
    string Name)
    : ICommand<Result<Guid>>;
