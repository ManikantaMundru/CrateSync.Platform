using CrateSync.Platform.BuildingBlocks.Application.Events;
using CrateSync.Platform.IntegrationContracts.Sales;
using CrateSync.Platform.Sales.Application.Abstractions;
using CrateSync.Platform.Sales.Domain.Orders.Events;
using MediatR;

namespace CrateSync.Platform.Sales.Application.EventHandlers;

internal sealed class SalesOrderConfirmedDomainEventHandler(
    ISalesOrderRepository repository,
    IPublisher publisher)
    : IDomainEventHandler<SalesOrderConfirmedDomainEvent>
{
    public async Task Handle(
        SalesOrderConfirmedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(
            notification.SalesOrderId,
            notification.TenantId,
            cancellationToken);

        if (order is null)
            return;

        var lines = order.Lines
            .Select(x =>
                new SalesOrderLineIntegrationContract(
                    x.Id.Value,
                    x.ProductVarietyId,
                    x.Quantity.Value))
            .ToArray();

        await publisher.Publish(
            new SalesOrderConfirmedIntegrationEvent(
                order.TenantId,
                order.Id.Value,
                order.WarehouseId,
                lines,
                notification.OccurredOnUtc),
            cancellationToken);
    }
}
