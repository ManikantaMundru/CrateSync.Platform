using CrateSync.Platform.Api.IntegrationTests.Infrastructure;
using CrateSync.Platform.Api.Requests.Inventory;
using CrateSync.Platform.Inventory.Application.Stock.GetStock;
using CrateSync.Platform.Inventory.Application.Stock.SearchStock;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CrateSync.Platform.Api.IntegrationTests.Journeys;

[Collection(ApiIntegrationCollection.Name)]
public sealed class InventoryOriginJourneyTests(
    CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task SameVariety_FromDifferentOrigins_ShouldRemainSeparateThroughTransfer()
    {
        await factory.ResetAsync();

        using var client = factory.CreateClient();

        var sourceWarehouseId = Guid.NewGuid();
        var destinationWarehouseId = Guid.NewGuid();
        var productVarietyId = Guid.NewGuid();

        await JourneyTestData.SeedInventoryAsync(
            factory,
            sourceWarehouseId,
            productVarietyId,
            100,
            "ZA");

        await JourneyTestData.SeedInventoryAsync(
            factory,
            sourceWarehouseId,
            productVarietyId,
            200,
            "PL");

        var stockResponse = await client.GetAsync(
            $"/api/inventory?warehouseId={sourceWarehouseId}");

        stockResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var positions =
            (await stockResponse.Content
                .ReadFromJsonAsync<List<StockSummaryResponse>>())!;

        positions.Count.ShouldBe(2);
        positions.ShouldContain(
            x =>
                x.ProductVarietyId == productVarietyId &&
                x.OriginCountry == "ZA" &&
                x.OnHand == 100);
        positions.ShouldContain(
            x =>
                x.ProductVarietyId == productVarietyId &&
                x.OriginCountry == "PL" &&
                x.OnHand == 200);

        var transferId = Guid.NewGuid();

        var transferResponse =
            await client.PostAsJsonAsync(
                "/api/inventory/transfers",
                new TransferStockRequest(
                    transferId,
                    sourceWarehouseId,
                    destinationWarehouseId,
                    productVarietyId,
                    "ZA",
                    40,
                    "TRF-ORIGIN-001",
                    null));

        transferResponse.StatusCode.ShouldBe(
            HttpStatusCode.NoContent);

        var sourceZa =
            await JourneyTestData.GetStockAsync(
                client,
                sourceWarehouseId,
                productVarietyId,
                "ZA");

        var sourcePl =
            await JourneyTestData.GetStockAsync(
                client,
                sourceWarehouseId,
                productVarietyId,
                "PL");

        var destinationZa =
            await JourneyTestData.GetStockAsync(
                client,
                destinationWarehouseId,
                productVarietyId,
                "ZA");

        sourceZa.OnHand.ShouldBe(60);
        sourcePl.OnHand.ShouldBe(200);
        destinationZa.OnHand.ShouldBe(40);
        destinationZa.OriginCountry.ShouldBe("ZA");
    }
}
