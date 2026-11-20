using CrateSync.Platform.BuildingBlocks.Application.Events;
using CrateSync.Platform.IntegrationContracts.Sales;
using CrateSync.Platform.Inventory.Application.Abstractions;

namespace CrateSync.Platform.Inventory.Application.EventHandlers;

internal sealed class SalesOrderCancelledIntegrationEventHandler(
    IInventoryBalanceRepository repository,
    IInventoryUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IIntegrationEventHandler<SalesOrderCancelledIntegrationEvent>
{
    public async Task Handle(
        SalesOrderCancelledIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        var balances = await repository.GetBySalesOrderIdAsync(
            notification.TenantId,
            notification.SalesOrderId,
            cancellationToken);

        if (balances.Count == 0) return;

        var now = timeProvider.GetUtcNow();
        var changed = false;

        foreach (var balance in balances)
            changed |= balance.ReleaseForOrder(notification.SalesOrderId, now);

        if (changed)
            await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
