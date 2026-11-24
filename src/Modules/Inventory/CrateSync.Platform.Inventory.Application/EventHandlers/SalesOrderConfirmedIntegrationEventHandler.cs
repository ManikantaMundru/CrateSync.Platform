using CrateSync.Platform.BuildingBlocks.Application.Events;
using CrateSync.Platform.IntegrationContracts.Inventory;
using CrateSync.Platform.IntegrationContracts.Sales;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Domain.ValueObjects;
using MediatR;

namespace CrateSync.Platform.Inventory.Application.EventHandlers;

internal sealed class SalesOrderConfirmedIntegrationEventHandler(
    IInventoryBalanceRepository repository,
    IInventoryUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IPublisher publisher)
    : IIntegrationEventHandler<SalesOrderConfirmedIntegrationEvent>
{
    public async Task Handle(
        SalesOrderConfirmedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        var balances = new List<(InventoryBalance Balance, int Quantity)>();

        foreach (var line in notification.Lines)
        {
            var candidates =
                await repository.GetByVarietyAsync(
                    notification.TenantId,
                    notification.WarehouseId,
                    line.ProductVarietyId,
                    cancellationToken);

            var existingBalances =
                candidates
                    .Where(x =>
                        x.Reservations.Any(
                            r =>
                                r.SalesOrderId ==
                                notification.SalesOrderId))
                    .ToArray();

            if (existingBalances.Length > 1)
            {
                await PublishFailure(
                    notification,
                    $"Sales order '{notification.SalesOrderId}' has reservations for multiple origins of product variety '{line.ProductVarietyId}'.",
                    cancellationToken);
                return;
            }

            InventoryBalance? balance =
                existingBalances.SingleOrDefault();

            if (balance is null)
            {
                var availableOrigins =
                    candidates
                        .Where(x => x.Available > 0)
                        .ToArray();

                if (availableOrigins.Length == 0)
                {
                    await PublishFailure(
                        notification,
                        $"Inventory does not exist for product variety '{line.ProductVarietyId}' at warehouse '{notification.WarehouseId}'.",
                        cancellationToken);
                    return;
                }

                if (availableOrigins.Length > 1)
                {
                    var origins =
                        string.Join(
                            ", ",
                            availableOrigins
                                .Select(x => x.CountryOfOrigin)
                                .OrderBy(x => x));

                    await PublishFailure(
                        notification,
                        $"Multiple countries of origin are available for product variety '{line.ProductVarietyId}' ({origins}). Select a country of origin on the sales order before confirming it.",
                        cancellationToken);
                    return;
                }

                balance = availableOrigins[0];
            }

            var existing =
                balance.Reservations.SingleOrDefault(
                    x =>
                        x.SalesOrderId ==
                        notification.SalesOrderId);

            if (existing is not null &&
                existing.Status ==
                    StockReservationStatus.Active &&
                existing.Quantity == line.Quantity)
            {
                balances.Add(
                    (balance, line.Quantity));
                continue;
            }

            if (balance.Available < line.Quantity)
            {
                await PublishFailure(
                    notification,
                    $"Insufficient {balance.CountryOfOrigin} stock for product variety '{line.ProductVarietyId}'. Available: {balance.Available}, requested: {line.Quantity}.",
                    cancellationToken);
                return;
            }

            balances.Add((balance, line.Quantity));
        }

        var now = timeProvider.GetUtcNow();

        foreach (var item in balances)
            item.Balance.ReserveForOrder(notification.SalesOrderId, new CrateQuantity(item.Quantity), now);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publisher.Publish(
            new StockReservedIntegrationEvent(notification.TenantId, notification.SalesOrderId, now),
            cancellationToken);
    }

    private Task PublishFailure(
        SalesOrderConfirmedIntegrationEvent notification,
        string reason,
        CancellationToken cancellationToken)
    {
        return publisher.Publish(
            new StockReservationFailedIntegrationEvent(
                notification.TenantId,
                notification.SalesOrderId,
                reason,
                timeProvider.GetUtcNow()),
            cancellationToken);
    }
}
