using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.TradingPartners.Application.Suppliers.GetSupplier;

public sealed record GetSupplierQuery(Guid SupplierId) : IQuery<Result<SupplierResponse>>;
