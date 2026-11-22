using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.TradingPartners.Application.Suppliers.ActivateSupplier;

public sealed record ActivateSupplierCommand(Guid SupplierId) : ICommand<Result>;
