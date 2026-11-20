using CrateSync.Platform.BuildingBlocks.Application.Events;
using CrateSync.Platform.IntegrationContracts.Sales;
using CrateSync.Platform.Sales.Domain.Orders.Events;
using MediatR;

namespace CrateSync.Platform.Sales.Application.EventHandlers;

internal sealed class SalesOrderCancelledDomainEventHandler(
    IPublisher publisher)
    : IDomainEventHandler<SalesOrderCancelledDomainEvent>
{
    public Task Handle(
        SalesOrderCancelledDomainEvent notification,
        CancellationToken cancellationToken)
    {
        return publisher.Publish(
            new SalesOrderCancelledIntegrationEvent(
                notification.TenantId,
                notification.SalesOrderId.Value,
                notification.OccurredOnUtc),
            cancellationToken);
    }
}
