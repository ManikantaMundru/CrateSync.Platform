using CrateSync.Platform.BuildingBlocks.Application.Events;
using CrateSync.Platform.IntegrationContracts.Sales;
using CrateSync.Platform.Sales.Domain.Orders.Events;
using MediatR;

namespace CrateSync.Platform.Sales.Application.EventHandlers;

internal sealed class SalesOrderDispatchRequestedDomainEventHandler(
    IPublisher publisher)
    : IDomainEventHandler<SalesOrderDispatchRequestedDomainEvent>
{
    public Task Handle(
        SalesOrderDispatchRequestedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        return publisher.Publish(
            new SalesOrderDispatchRequestedIntegrationEvent(
                notification.TenantId,
                notification.SalesOrderId.Value,
                notification.OccurredOnUtc),
            cancellationToken);
    }
}
