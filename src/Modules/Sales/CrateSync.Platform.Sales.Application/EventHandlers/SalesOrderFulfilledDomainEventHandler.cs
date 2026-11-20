using CrateSync.Platform.BuildingBlocks.Application.Events;
using CrateSync.Platform.IntegrationContracts.Sales;
using CrateSync.Platform.Sales.Application.Abstractions;
using CrateSync.Platform.Sales.Domain.Orders.Events;
using MediatR;

namespace CrateSync.Platform.Sales.Application.EventHandlers;

internal sealed class SalesOrderFulfilledDomainEventHandler(
    ISalesOrderRepository repository,
    IPublisher publisher)
    : IDomainEventHandler<SalesOrderFulfilledDomainEvent>
{
    public async Task Handle(
        SalesOrderFulfilledDomainEvent notification,
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
                new SalesOrderFulfilledLineIntegrationContract(
                    x.Id.Value,
                    x.ProductVarietyId,
                    x.Quantity.Value,
                    x.PackSize.Value,
                    x.UnitPrice.Amount,
                    x.UnitPrice.Currency.Code))
            .ToArray();

        await publisher.Publish(
            new SalesOrderFulfilledIntegrationEvent(
                order.TenantId,
                order.Id.Value,
                order.CustomerId,
                order.OrderNumber.Value,
                lines,
                notification.OccurredOnUtc),
            cancellationToken);
    }
}
