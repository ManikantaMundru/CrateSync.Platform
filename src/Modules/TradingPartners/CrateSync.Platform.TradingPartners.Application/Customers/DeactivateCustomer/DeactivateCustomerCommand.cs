using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.TradingPartners.Application.Customers.DeactivateCustomer;

public sealed record DeactivateCustomerCommand(Guid CustomerId) : ICommand<Result>;
