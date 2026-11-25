using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Inventory.Tests.Domain;

public sealed class WarehouseTransferTests
{
    [Fact]
    public void Transfer_ReservesThenDispatchesAndReceivesOnlyAcceptedStock()
    {
        var now = DateTimeOffset.UtcNow;
        var source = InventoryBalance.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "ZA", now);
        source.Receive(new CrateQuantity(100), StockReference.Shipment(Guid.NewGuid()), now);
        var transfer = WarehouseTransfer.Create(Guid.NewGuid(), source, Guid.NewGuid(), 40, "Transport", "Driver", "123", null, null, null, now);
        var reference = StockReference.WarehouseTransfer(transfer.Id);
        source.ReserveForTransfer(new CrateQuantity(40), reference, now);
        source.OnHand.Value.ShouldBe(100);
        source.Reserved.Value.ShouldBe(40);
        source.Available.ShouldBe(60);
        Should.Throw<InvalidOperationException>(() => transfer.Receive(40, 0, 0, null, now));
        transfer.Dispatch(now);
        source.DispatchTransfer(new CrateQuantity(40), reference, now);
        source.OnHand.Value.ShouldBe(60);
        source.Reserved.Value.ShouldBe(0);
        transfer.Receive(37, 2, 1, "Transport discrepancy", now);
        transfer.Status.ShouldBe("ReceivedWithDiscrepancies");
        var destination = InventoryBalance.Create(source.TenantId, transfer.DestinationWarehouseId, source.ProductVarietyId, source.OriginCountry, now);
        destination.TransferIn(new CrateQuantity(transfer.Accepted!.Value), reference, null, now);
        destination.OnHand.Value.ShouldBe(37);
    }
    [Fact]
    public void Cancel_ReleasesReservedStock()
    {
        var now = DateTimeOffset.UtcNow;
        var source = InventoryBalance.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "ZA", now);
        source.Receive(new CrateQuantity(100), StockReference.Shipment(Guid.NewGuid()), now);
        var transfer = WarehouseTransfer.Create(Guid.NewGuid(), source, Guid.NewGuid(), 40, "Transport", "Driver", "123", null, null, null, now);
        var reference = StockReference.WarehouseTransfer(transfer.Id);
        source.ReserveForTransfer(new CrateQuantity(40), reference, now);
        transfer.Cancel(now);
        source.ReleaseTransfer(new CrateQuantity(40), reference, now);
        source.Available.ShouldBe(100);
        source.Reserved.Value.ShouldBe(0);
        transfer.Status.ShouldBe("Cancelled");
    }
}
