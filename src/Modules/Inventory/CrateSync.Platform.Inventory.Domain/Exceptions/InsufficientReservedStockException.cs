using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Inventory.Domain.Exceptions;

public sealed class InsufficientReservedStockException(
    int reservedQuantity,
    int requestedQuantity) : DomainException(
        "Inventory.InsufficientReservedStock",
        $"Insufficient reserved stock. Reserved quantity is {reservedQuantity}, but {requestedQuantity} was requested.")
{
    public int ReservedQuantity { get; } = reservedQuantity;

    public int RequestedQuantity { get; } = requestedQuantity;
}
