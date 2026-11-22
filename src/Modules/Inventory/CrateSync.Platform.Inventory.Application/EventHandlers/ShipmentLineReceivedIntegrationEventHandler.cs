using CrateSync.Platform.BuildingBlocks.Application.Events;
using CrateSync.Platform.InboundLogistics.Contracts;
using CrateSync.Platform.Inventory.Application.Stock.ReceiveStock;
using MediatR;

namespace CrateSync.Platform.Inventory.Application.EventHandlers;

internal sealed class ShipmentLineReceivedIntegrationEventHandler(ISender sender)
    : IIntegrationEventHandler<ShipmentLineReceivedIntegrationEvent>
{
    public Task Handle(
        ShipmentLineReceivedIntegrationEvent notification,
        CancellationToken cancellationToken) =>
        sender.Send(
            new ReceiveStockCommand(
                notification.TenantId,
                notification.WarehouseId,
                notification.ProductVarietyId,
                notification.Quantity,
                notification.ShipmentId,
                notification.ReceiptId),
            cancellationToken);
}
