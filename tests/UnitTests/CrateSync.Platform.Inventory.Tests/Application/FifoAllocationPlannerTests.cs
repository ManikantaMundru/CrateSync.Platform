using CrateSync.Platform.Inventory.Application.Stock.PlanFifoAllocation;
using CrateSync.Platform.Inventory.Application.Stock.SearchStock;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Inventory.Tests.Application;

public sealed class FifoAllocationPlannerTests
{
    [Fact]
    public void Plan_SelectsOldestEligibleBatchesWithoutMixingPackaging()
    {
        var warehouse = Guid.NewGuid();
        var variety = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        StockSummaryResponse Row(string code, int count, int available, int age, decimal? price = 100m) =>
            new(Guid.NewGuid(), warehouse, variety, available, 0, available, now,
                OriginCountry: "ZA", BatchId: Guid.NewGuid(), BatchCode: code, FruitsPerPack: count,
                SellingPrice: price, SellingCurrency: "INR", OriginalReceivedAtUtc: now.AddDays(-age));
        var rows = new[] { Row("new",100,60,2), Row("old",100,30,10), Row("other-pack",120,100,20), Row("unpriced",100,100,30,null) };
        var plan = FifoAllocationPlanner.Plan(new(warehouse,variety,"ZA",100,null,100,"INR"), rows);
        plan.Lines.Select(x => x.BatchCode).ShouldBe(new[] { "old", "new" });
        plan.Allocated.ShouldBe(90);
        plan.Shortfall.ShouldBe(10);
    }
}
