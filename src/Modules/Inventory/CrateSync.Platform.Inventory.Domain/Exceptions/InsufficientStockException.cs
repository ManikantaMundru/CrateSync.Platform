using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Inventory.Domain.Exceptions;

public sealed class InsufficientStockException(
    int availableQuantity,
    int requestedQuantity) : DomainException(
        "Inventory.InsufficientStock",
        $"Insufficient available stock. Available quantity is {availableQuantity}, but {requestedQuantity} was requested.")
{
    public int AvailableQuantity { get; } = availableQuantity;

    public int RequestedQuantity { get; } = requestedQuantity;
}
