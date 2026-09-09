using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Catalog.Application.Products.DeactivateProduct;

public sealed record DeactivateProductCommand(
    Guid ProductId)
    : ICommand<Result>;
