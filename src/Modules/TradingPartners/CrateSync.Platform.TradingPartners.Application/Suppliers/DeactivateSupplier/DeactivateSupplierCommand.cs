using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.TradingPartners.Application.Suppliers.DeactivateSupplier;

public sealed record DeactivateSupplierCommand(Guid SupplierId) : ICommand<Result>;
