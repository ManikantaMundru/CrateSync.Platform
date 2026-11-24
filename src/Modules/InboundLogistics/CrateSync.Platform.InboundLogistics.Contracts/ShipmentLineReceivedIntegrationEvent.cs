using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.InboundLogistics.Contracts;

public sealed record ShipmentLineReceivedIntegrationEvent(
    Guid ReceiptId,
    Guid ShipmentId,
    Guid TenantId,
    Guid WarehouseId,
    Guid ProductVarietyId,
    string CountryOfOrigin,
    int Quantity,
    DateTimeOffset OccurredAtUtc) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
