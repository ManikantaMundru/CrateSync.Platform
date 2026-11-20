using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Application.Stock.SearchStock;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Inventory.Tests.Application.SearchStock;

public sealed class SearchStockQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Search_Results()
    {
        var readService = Substitute.For<IInventoryReadService>();
        var tenantContext = Substitute.For<ITenantContext>();

        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        tenantContext.TenantId.Returns(tenantId);

        IReadOnlyCollection<StockSummaryResponse> expected =
        [
            new(
                Guid.NewGuid(),
                warehouseId,
                Guid.NewGuid(),
                100,
                20,
                80,
                DateTimeOffset.UtcNow)
        ];

        readService
            .SearchStockAsync(
                tenantId,
                warehouseId,
                null,
                TestContext.Current.CancellationToken)
            .Returns(expected);

        var handler = new SearchStockQueryHandler(
            readService,
            tenantContext);

        var result = await handler.Handle(
            new SearchStockQuery(
                warehouseId,
                null),
            TestContext.Current.CancellationToken);

        result.ShouldBe(expected);
    }
}
