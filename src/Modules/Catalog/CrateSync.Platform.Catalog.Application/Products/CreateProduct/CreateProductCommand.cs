using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Catalog.Application.Products.CreateProduct;

public sealed record CreateProductCommand(string Name): ICommand<Result<Guid>>;
