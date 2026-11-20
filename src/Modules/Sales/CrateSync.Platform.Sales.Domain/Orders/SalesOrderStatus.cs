namespace CrateSync.Platform.Sales.Domain.Orders;

public enum SalesOrderStatus
{
    Draft = 1,
    Confirmed = 2,
    Allocated = 3,
    Dispatched = 4,
    Fulfilled = 5,
    Cancelled = 6
}
