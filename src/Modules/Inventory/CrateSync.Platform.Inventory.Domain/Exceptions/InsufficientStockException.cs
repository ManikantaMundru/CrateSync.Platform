using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Inventory.Domain.Exceptions;

public sealed class InsufficientStockException : DomainException
{
    public InsufficientStockException(int availableQuantity, int requestedQuantity)
        : base(
            "Inventory.InsufficientStock",
            $"Insufficient available stock. Available quantity is {availableQuantity}, but {requestedQuantity} was requested.")
    {
        AvailableQuantity = availableQuantity;
        RequestedQuantity = requestedQuantity;
    }

    public int AvailableQuantity { get; }
    public int RequestedQuantity { get; }
}
