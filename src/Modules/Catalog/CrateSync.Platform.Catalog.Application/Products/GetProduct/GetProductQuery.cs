using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.Catalog.Application.Products.GetProduct;

public sealed record GetProductQuery(
    Guid ProductId)
    : IQuery<Result<ProductResponse>>;
