using CrateSync.Platform.BuildingBlocks.Application.Events;
using CrateSync.Platform.IntegrationContracts.Inventory;
using CrateSync.Platform.Sales.Application.Orders.AllocateOrder;
using MediatR;

namespace CrateSync.Platform.Sales.Application.EventHandlers;

internal sealed class StockReservedIntegrationEventHandler(ISender sender)
    : IIntegrationEventHandler<StockReservedIntegrationEvent>
{
    public Task Handle(StockReservedIntegrationEvent notification, CancellationToken cancellationToken) =>
        sender.Send(new AllocateSalesOrderCommand(notification.SalesOrderId), cancellationToken);
}
