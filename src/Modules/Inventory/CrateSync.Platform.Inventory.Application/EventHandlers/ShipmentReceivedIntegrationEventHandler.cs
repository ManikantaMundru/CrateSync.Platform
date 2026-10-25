using CrateSync.Platform.BuildingBlocks.Application.Events;
using CrateSync.Platform.InboundLogistics.Contracts;
using CrateSync.Platform.Inventory.Application.Stock.ReceiveStock;
using MediatR;

namespace CrateSync.Platform.Inventory.Application.EventHandlers;

internal sealed class ShipmentReceivedIntegrationEventHandler(ISender sender) :
    IIntegrationEventHandler<ShipmentReceivedIntegrationEvent>
{
    private readonly ISender _sender = sender;

    public async Task Handle(
        ShipmentReceivedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        foreach (var line in notification.Lines)
        {
            if (line.ReceivedQuantity <= 0)
                continue;

            await _sender.Send(
                new ReceiveStockCommand(
                    notification.WarehouseId,
                    line.ProductVarietyId,
                    line.ReceivedQuantity,
                    notification.ShipmentId),
                cancellationToken);
        }
    }
}
