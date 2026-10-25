namespace CrateSync.Platform.Inventory.Domain.Stock;

public enum StockMovementType
{
    Received = 1,
    Reserved = 2,
    ReservationReleased = 3,
    Dispatched = 4,
    Adjusted = 5
}
