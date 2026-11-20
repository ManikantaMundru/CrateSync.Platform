using CrateSync.Platform.Sales.Domain.Orders;

namespace CrateSync.Platform.Sales.Application.Abstractions;

public interface ISalesOrderRepository
{
    Task<SalesOrder?> GetByIdAsync(
        SalesOrderId id,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<bool> OrderNumberExistsAsync(
        Guid tenantId,
        string orderNumber,
        CancellationToken cancellationToken = default);

    void Add(SalesOrder order);
}
