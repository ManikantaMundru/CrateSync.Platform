using CrateSync.Platform.Api.IntegrationTests.Infrastructure;
using Shouldly;
using System.Net;
using Xunit;

namespace CrateSync.Platform.Api.IntegrationTests.Journeys;

[Collection(ApiIntegrationCollection.Name)]
public sealed class SalesCancellationJourneyTests(
    CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task CancellingAllocatedOrder_ShouldReleaseOnlyThatOrdersReservation()
    {
        // Arrange
        await factory.ResetAsync();

        using var client = factory.CreateClient();

        var warehouseId = Guid.NewGuid();
        var productVarietyId = Guid.NewGuid();

        await JourneyTestData.SeedInventoryAsync(
            factory,
            warehouseId,
            productVarietyId,
            100);

        var orderA =
            await JourneyTestData.CreateSalesOrderAsync(
                client,
                warehouseId);

        await JourneyTestData.AddSalesOrderLineAsync(
            client,
            orderA,
            productVarietyId,
            30);

        var orderB =
            await JourneyTestData.CreateSalesOrderAsync(
                client,
                warehouseId);

        await JourneyTestData.AddSalesOrderLineAsync(
            client,
            orderB,
            productVarietyId,
            20);

        (await client.PatchAsync(
            $"/api/sales-orders/{orderA}/confirm",
            null)).EnsureSuccessStatusCode();

        (await client.PatchAsync(
            $"/api/sales-orders/{orderB}/confirm",
            null)).EnsureSuccessStatusCode();

        var beforeCancel =
            await JourneyTestData.GetStockAsync(
                client,
                warehouseId,
                productVarietyId);

        beforeCancel.Reserved.ShouldBe(50);

        // Act
        var cancel =
            await client.PatchAsync(
                $"/api/sales-orders/{orderA}/cancel",
                null);

        // Assert
        cancel.StatusCode.ShouldBe(
            HttpStatusCode.NoContent);

        var cancelledOrder =
            await JourneyTestData.GetSalesOrderAsync(
                client,
                orderA);

        var survivingOrder =
            await JourneyTestData.GetSalesOrderAsync(
                client,
                orderB);

        cancelledOrder.Status.ShouldBe("Cancelled");
        survivingOrder.Status.ShouldBe("Allocated");

        var stock =
            await JourneyTestData.GetStockAsync(
                client,
                warehouseId,
                productVarietyId);

        stock.OnHand.ShouldBe(100);
        stock.Reserved.ShouldBe(20);
        stock.Available.ShouldBe(80);
    }
}
