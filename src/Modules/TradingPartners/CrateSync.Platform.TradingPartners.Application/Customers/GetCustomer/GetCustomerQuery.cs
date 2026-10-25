using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.TradingPartners.Application.Customers.GetCustomer;

public sealed record GetCustomerQuery(Guid CustomerId) : IQuery<Result<CustomerResponse>>;
