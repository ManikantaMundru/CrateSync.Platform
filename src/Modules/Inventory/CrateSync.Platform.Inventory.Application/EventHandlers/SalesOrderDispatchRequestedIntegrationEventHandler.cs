using CrateSync.Platform.BuildingBlocks.Application.Events;
using CrateSync.Platform.IntegrationContracts.Inventory;
using CrateSync.Platform.IntegrationContracts.Sales;
using CrateSync.Platform.Inventory.Application.Abstractions;
using MediatR;

namespace CrateSync.Platform.Inventory.Application.EventHandlers;

internal sealed class SalesOrderDispatchRequestedIntegrationEventHandler(
    IInventoryBalanceRepository repository,
    IInventoryUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IPublisher publisher)
    : IIntegrationEventHandler<SalesOrderDispatchRequestedIntegrationEvent>
{
    public async Task Handle(
        SalesOrderDispatchRequestedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        var balances = await repository.GetBySalesOrderIdAsync(
            notification.TenantId,
            notification.SalesOrderId,
            cancellationToken);

        if (balances.Count == 0)
            throw new InvalidOperationException(
                $"No inventory reservations exist for sales order '{notification.SalesOrderId}'.");

        var now = timeProvider.GetUtcNow();
        var changed = false;

        foreach (var balance in balances)
            changed |= balance.DispatchForOrder(notification.SalesOrderId, now);

        if (changed)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        await publisher.Publish(
            new StockDispatchedIntegrationEvent(notification.TenantId, notification.SalesOrderId, now),
            cancellationToken);
    }
}
