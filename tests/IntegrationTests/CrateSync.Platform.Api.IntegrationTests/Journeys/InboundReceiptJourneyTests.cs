using CrateSync.Platform.Api.IntegrationTests.Infrastructure;
using CrateSync.Platform.Api.Requests.InboundShipments;
using CrateSync.Platform.Api.Responses.InboundShipments;
using CrateSync.Platform.InboundLogistics.Application.Shipments.GetShipment;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CrateSync.Platform.Api.IntegrationTests.Journeys;

[Collection(ApiIntegrationCollection.Name)]
public sealed class InboundReceiptJourneyTests(
    CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task InboundShipment_ShouldSplitReceivedStockAcrossWarehouses()
    {
        await factory.ResetAsync();

        using var client = factory.CreateClient();

        var warehouseAId = Guid.NewGuid();
        var warehouseBId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productVarietyId = Guid.NewGuid();

        var createResponse =
            await client.PostAsJsonAsync(
                "/api/InboundShipment",
                new CreateInboundShipmentRequest(
                    Guid.NewGuid(),
                    $"TRACK-{Guid.NewGuid():N}",
                    "ZA",
                    DateTimeOffset.UtcNow.AddDays(5)));

        createResponse.StatusCode.ShouldBe(
            HttpStatusCode.Created);

        var shipmentId =
            (await createResponse.Content
                .ReadFromJsonAsync<CreateInboundShipmentResponse>())!
            .Id;

        var addLineResponse =
            await client.PostAsJsonAsync(
                $"/api/InboundShipment/{shipmentId}/lines",
                new AddShipmentLineRequest(
                    productId,
                    productVarietyId,
                    $"BATCH-{Guid.NewGuid():N}",
                    40,
                    18m,
                    250m,
                    "ZAR"));

        addLineResponse.StatusCode.ShouldBe(
            HttpStatusCode.Created);

        var lineId =
            (await addLineResponse.Content
                .ReadFromJsonAsync<AddShipmentLineResponse>())!
            .Id;

        (await client.PatchAsync(
            $"/api/InboundShipment/{shipmentId}/ship",
            null)).EnsureSuccessStatusCode();

        (await client.PatchAsync(
            $"/api/InboundShipment/{shipmentId}/arrive",
            null)).EnsureSuccessStatusCode();

        var receiveA =
            await client.PostAsJsonAsync(
                $"/api/InboundShipment/{shipmentId}/receive",
                new ReceiveShipmentRequest(
                    Guid.NewGuid(),
                    lineId,
                    warehouseAId,
                    15));

        receiveA.StatusCode.ShouldBe(
            HttpStatusCode.NoContent);

        var receiveB =
            await client.PostAsJsonAsync(
                $"/api/InboundShipment/{shipmentId}/receive",
                new ReceiveShipmentRequest(
                    Guid.NewGuid(),
                    lineId,
                    warehouseBId,
                    25));

        receiveB.StatusCode.ShouldBe(
            HttpStatusCode.NoContent);

        var shipment =
            await client.GetFromJsonAsync<ShipmentResponse>(
                $"/api/InboundShipment/{shipmentId}");

        shipment.ShouldNotBeNull();
        shipment.Status.ShouldBe("Received");
        shipment.Lines.Single().ReceivedQuantity.ShouldBe(40);
        shipment.Receipts.ShouldNotBeNull();
        shipment.Receipts.Count.ShouldBe(2);
        shipment.Receipts.ShouldContain(
            x =>
                x.WarehouseId == warehouseAId &&
                x.Quantity == 15);
        shipment.Receipts.ShouldContain(
            x =>
                x.WarehouseId == warehouseBId &&
                x.Quantity == 25);

        var stockA =
            await JourneyTestData.GetStockAsync(
                client,
                warehouseAId,
                productVarietyId);

        stockA.OnHand.ShouldBe(15);

        var stockB =
            await JourneyTestData.GetStockAsync(
                client,
                warehouseBId,
                productVarietyId);

        stockB.OnHand.ShouldBe(25);
    }
}
