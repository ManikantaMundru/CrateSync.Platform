using CrateSync.Platform.Sales.Application.Abstractions;
using CrateSync.Platform.Sales.Domain.Orders;
using CrateSync.Platform.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Sales.Infrastructure.Repositories;

internal sealed class SalesOrderRepository(SalesDbContext dbContext) : ISalesOrderRepository
{
    public Task<SalesOrder?> GetByIdAsync(
        SalesOrderId id,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.SalesOrders
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId,
                cancellationToken);
    }

    public Task<bool> OrderNumberExistsAsync(
        Guid tenantId,
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        var value = new OrderNumber(orderNumber);

        return dbContext.SalesOrders.AnyAsync(
            x => x.TenantId == tenantId && x.OrderNumber == value,
            cancellationToken);
    }

    public void Add(SalesOrder order) => dbContext.SalesOrders.Add(order);
}
