using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.TradingPartners.Application.Customers.ActivateCustomer;

public sealed record ActivateCustomerCommand(Guid CustomerId) : ICommand<Result>;
