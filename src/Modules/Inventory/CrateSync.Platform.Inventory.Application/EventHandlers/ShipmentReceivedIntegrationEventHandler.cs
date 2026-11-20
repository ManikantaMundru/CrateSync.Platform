using CrateSync.Platform.BuildingBlocks.Application.Events;
using CrateSync.Platform.IntegrationContracts.InboundLogistics;
using CrateSync.Platform.Inventory.Application.Stock.ReceiveStock;
using MediatR;

namespace CrateSync.Platform.Inventory.Application.EventHandlers;

internal sealed class ShipmentReceivedIntegrationEventHandler(ISender sender)
    : IIntegrationEventHandler<ShipmentReceivedIntegrationEvent>
{
    public async Task Handle(ShipmentReceivedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        foreach (var line in notification.Lines)
        {
            if (line.ReceivedQuantity <= 0) continue;

            await sender.Send(
                new ReceiveStockCommand(
                    notification.TenantId,
                    notification.WarehouseId,
                    line.ProductVarietyId,
                    line.ReceivedQuantity,
                    notification.ShipmentId),
                cancellationToken);
        }
    }
}
