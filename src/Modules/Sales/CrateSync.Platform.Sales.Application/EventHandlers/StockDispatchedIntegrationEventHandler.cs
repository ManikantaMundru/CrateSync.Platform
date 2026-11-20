using CrateSync.Platform.BuildingBlocks.Application.Events;
using CrateSync.Platform.IntegrationContracts.Inventory;
using CrateSync.Platform.Sales.Application.Orders.DispatchOrder;
using MediatR;

namespace CrateSync.Platform.Sales.Application.EventHandlers;

internal sealed class StockDispatchedIntegrationEventHandler(ISender sender)
    : IIntegrationEventHandler<StockDispatchedIntegrationEvent>
{
    public Task Handle(StockDispatchedIntegrationEvent notification, CancellationToken cancellationToken) =>
        sender.Send(new DispatchSalesOrderCommand(notification.SalesOrderId), cancellationToken);
}
