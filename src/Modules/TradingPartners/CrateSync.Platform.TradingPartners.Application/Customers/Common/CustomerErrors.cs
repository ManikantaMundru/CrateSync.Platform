using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.TradingPartners.Domain.Customers;

namespace CrateSync.Platform.TradingPartners.Application.Customers.Common;

public static class CustomerErrors
{
    public static Error NotFound(CustomerId customerId) =>
        Error.NotFound("TradingPartners.Customer.NotFound", $"Customer '{customerId.Value}' was not found.");

    public static Error DuplicateCommercialIdentifier(string commercialIdentifier) =>
        Error.Conflict(
            "TradingPartners.Customer.DuplicateCommercialIdentifier",
            $"A customer with commercial identifier '{commercialIdentifier}' already exists.");
}
