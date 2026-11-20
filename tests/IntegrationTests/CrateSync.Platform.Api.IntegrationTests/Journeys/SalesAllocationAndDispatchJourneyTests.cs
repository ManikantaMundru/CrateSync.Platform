using CrateSync.Platform.Api.IntegrationTests.Infrastructure;
using Shouldly;
using System.Net;
using Xunit;

namespace CrateSync.Platform.Api.IntegrationTests.Journeys;

[Collection(ApiIntegrationCollection.Name)]
public sealed class SalesAllocationAndDispatchJourneyTests(
    CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task SalesOrder_WithAvailableStock_ShouldAllocateThenDispatchAndDeductInventoryOnce()
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

        var orderId =
            await JourneyTestData.CreateSalesOrderAsync(
                client,
                warehouseId);

        await JourneyTestData.AddSalesOrderLineAsync(
            client,
            orderId,
            productVarietyId,
            30);

        // Act - confirm / reserve
        var confirm =
            await client.PatchAsync(
                $"/api/sales-orders/{orderId}/confirm",
                null);

        // Assert - allocated, physical stock unchanged
        confirm.StatusCode.ShouldBe(
            HttpStatusCode.NoContent);

        var allocatedOrder =
            await JourneyTestData.GetSalesOrderAsync(
                client,
                orderId);

        allocatedOrder.Status.ShouldBe("Allocated");

        var reservedStock =
            await JourneyTestData.GetStockAsync(
                client,
                warehouseId,
                productVarietyId);

        reservedStock.OnHand.ShouldBe(100);
        reservedStock.Reserved.ShouldBe(30);
        reservedStock.Available.ShouldBe(70);

        // Act - dispatch
        var dispatch =
            await client.PatchAsync(
                $"/api/sales-orders/{orderId}/dispatch",
                null);

        // Assert - Sales and Inventory both move
        dispatch.StatusCode.ShouldBe(
            HttpStatusCode.NoContent);

        var dispatchedOrder =
            await JourneyTestData.GetSalesOrderAsync(
                client,
                orderId);

        dispatchedOrder.Status.ShouldBe("Dispatched");

        var dispatchedStock =
            await JourneyTestData.GetStockAsync(
                client,
                warehouseId,
                productVarietyId);

        dispatchedStock.OnHand.ShouldBe(70);
        dispatchedStock.Reserved.ShouldBe(0);
        dispatchedStock.Available.ShouldBe(70);

        // Act - repeat dispatch request
        var repeatedDispatch =
            await client.PatchAsync(
                $"/api/sales-orders/{orderId}/dispatch",
                null);

        // Assert - invalid state and no double deduction
        repeatedDispatch.StatusCode.ShouldBe(
            HttpStatusCode.Conflict);

        var stockAfterReplay =
            await JourneyTestData.GetStockAsync(
                client,
                warehouseId,
                productVarietyId);

        stockAfterReplay.OnHand.ShouldBe(70);
        stockAfterReplay.Reserved.ShouldBe(0);
    }

    [Fact]
    public async Task SalesOrder_WhenOneLineHasInsufficientStock_ShouldRemainConfirmedAndReserveNothing()
    {
        // Arrange
        await factory.ResetAsync();

        using var client = factory.CreateClient();

        var warehouseId = Guid.NewGuid();
        var varietyA = Guid.NewGuid();
        var varietyB = Guid.NewGuid();

        await JourneyTestData.SeedInventoryAsync(
            factory,
            warehouseId,
            varietyA,
            100);

        await JourneyTestData.SeedInventoryAsync(
            factory,
            warehouseId,
            varietyB,
            5);

        var orderId =
            await JourneyTestData.CreateSalesOrderAsync(
                client,
                warehouseId);

        await JourneyTestData.AddSalesOrderLineAsync(
            client,
            orderId,
            varietyA,
            30);

        await JourneyTestData.AddSalesOrderLineAsync(
            client,
            orderId,
            varietyB,
            20);

        // Act
        var confirm =
            await client.PatchAsync(
                $"/api/sales-orders/{orderId}/confirm",
                null);

        // Assert
        confirm.StatusCode.ShouldBe(
            HttpStatusCode.NoContent);

        var order =
            await JourneyTestData.GetSalesOrderAsync(
                client,
                orderId);

        order.Status.ShouldBe("Confirmed");
        order.ReservationFailureReason.ShouldNotBeNullOrWhiteSpace();
        order.ReservationFailureReason.ShouldContain(
            "Insufficient stock");

        var stockA =
            await JourneyTestData.GetStockAsync(
                client,
                warehouseId,
                varietyA);

        var stockB =
            await JourneyTestData.GetStockAsync(
                client,
                warehouseId,
                varietyB);

        stockA.Reserved.ShouldBe(0);
        stockB.Reserved.ShouldBe(0);
    }
}
