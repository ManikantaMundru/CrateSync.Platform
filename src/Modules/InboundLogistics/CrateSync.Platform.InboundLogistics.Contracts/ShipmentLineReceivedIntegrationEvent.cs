using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.InboundLogistics.Contracts;

public sealed record ShipmentLineReceivedIntegrationEvent(
    Guid ReceiptId,
    Guid ShipmentId,
    Guid TenantId,
    Guid WarehouseId,
    Guid ProductVarietyId,
    string OriginCountry,
    int Quantity,
    DateTimeOffset OccurredAtUtc, Guid BatchId = default, string? BatchCode = null, decimal? PackWeightKg = null, int? FruitsPerPack = null) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
