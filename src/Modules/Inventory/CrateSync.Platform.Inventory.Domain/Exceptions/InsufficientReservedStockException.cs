using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Inventory.Domain.Exceptions;

public sealed class InsufficientReservedStockException : DomainException
{
    public InsufficientReservedStockException(int reservedQuantity, int requestedQuantity)
        : base(
            "Inventory.InsufficientReservedStock",
            $"Insufficient reserved stock. Reserved quantity is {reservedQuantity}, but {requestedQuantity} was requested.")
    {
        ReservedQuantity = reservedQuantity;
        RequestedQuantity = requestedQuantity;
    }

    public int ReservedQuantity { get; }
    public int RequestedQuantity { get; }
}
