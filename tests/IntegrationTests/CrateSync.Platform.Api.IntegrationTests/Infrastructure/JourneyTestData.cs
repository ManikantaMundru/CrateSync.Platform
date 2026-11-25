using CrateSync.Platform.Api.Requests.Sales;
using CrateSync.Platform.Inventory.Application.Stock.GetStock;
using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Domain.ValueObjects;
using CrateSync.Platform.Inventory.Infrastructure.Persistence;
using CrateSync.Platform.Sales.Application.Orders.GetOrder;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace CrateSync.Platform.Api.IntegrationTests.Infrastructure;

internal static class JourneyTestData
{
    public static async Task SeedInventoryAsync(
        CustomWebApplicationFactory factory,
        Guid warehouseId,
        Guid productVarietyId,
        int quantity,
        string originCountry = "ZA")
    {
        using var scope = factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<InventoryDbContext>();

        var now = DateTimeOffset.UtcNow;

        var balance = InventoryBalance.Create(
            factory.TenantId,
            warehouseId,
            productVarietyId,
            originCountry,
            now.AddMinutes(-1));

        balance.Receive(
            new CrateQuantity(quantity),
            StockReference.Shipment(Guid.NewGuid()),
            now);

        dbContext.InventoryBalances.Add(balance);

        await dbContext.SaveChangesAsync();
    }

    public static async Task<Guid> CreateSalesOrderAsync(
        HttpClient client,
        Guid warehouseId)
    {
        var response = await client.PostAsJsonAsync(
            "/api/sales-orders",
            new CreateSalesOrderRequest(
                Guid.NewGuid(),
                warehouseId,
                $"SO-{Guid.NewGuid():N}",
                "1 Test Street",
                null,
                "Cape Town",
                "Western Cape",
                "7441",
                "South Africa"));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    public static async Task<Guid> AddSalesOrderLineAsync(
        HttpClient client,
        Guid salesOrderId,
        Guid productVarietyId,
        int quantity,
        string originCountry = "ZA")
    {
        var response = await client.PostAsJsonAsync(
            $"/api/sales-orders/{salesOrderId}/lines",
            new AddSalesOrderLineRequest(
                productVarietyId,
                quantity,
                18m,
                250m,
                "ZAR",
                originCountry));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    public static async Task<SalesOrderResponse> GetSalesOrderAsync(
        HttpClient client,
        Guid salesOrderId)
    {
        var response =
            await client.GetAsync(
                $"/api/sales-orders/{salesOrderId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        return (await response.Content
            .ReadFromJsonAsync<SalesOrderResponse>())!;
    }

    public static async Task<StockResponse> GetStockAsync(
        HttpClient client,
        Guid warehouseId,
        Guid productVarietyId,
        string originCountry = "ZA", Guid batchId = default)
    {
        var response =
            await client.GetAsync(
                $"/api/inventory/stock?warehouseId={warehouseId}&productVarietyId={productVarietyId}&originCountry={originCountry}&batchId={batchId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        return (await response.Content
            .ReadFromJsonAsync<StockResponse>())!;
    }
}
