using CrateSync.Platform.Api.IntegrationTests.Infrastructure;
using CrateSync.Platform.Api.Requests.InboundShipments;
using CrateSync.Platform.Api.Responses.InboundShipments;
using CrateSync.Platform.InboundLogistics.Application.Shipments.GetShipment;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
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
    public async Task InboundShipment_ShouldSplitPortDeliveriesAcrossWarehouses()
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

        var clearanceResponse =
            await client.PostAsJsonAsync(
                $"/api/InboundShipment/{shipmentId}/clearance-events",
                new RecordClearanceEventRequest(
                    ClearanceAuthority.Customs,
                    "Cleared",
                    "BOE-001",
                    "Cleared for warehouse dispatch"));

        clearanceResponse.StatusCode.ShouldBe(
            HttpStatusCode.Created);

        var deliveryAId =
            await CreateDeliveryAsync(
                client,
                shipmentId,
                warehouseAId,
                lineId,
                15,
                "TRUCK-A");

        var deliveryBId =
            await CreateDeliveryAsync(
                client,
                shipmentId,
                warehouseBId,
                lineId,
                25,
                "TRUCK-B");

        (await client.PatchAsync(
            $"/api/InboundShipment/{shipmentId}/warehouse-deliveries/{deliveryAId}/dispatch",
            null)).EnsureSuccessStatusCode();

        (await client.PatchAsync(
            $"/api/InboundShipment/{shipmentId}/warehouse-deliveries/{deliveryBId}/dispatch",
            null)).EnsureSuccessStatusCode();

        (await client.PatchAsync(
            $"/api/InboundShipment/{shipmentId}/warehouse-deliveries/{deliveryAId}/receive",
            null)).EnsureSuccessStatusCode();

        (await client.PatchAsync(
            $"/api/InboundShipment/{shipmentId}/warehouse-deliveries/{deliveryBId}/receive",
            null)).EnsureSuccessStatusCode();

        var shipment =
            await client.GetFromJsonAsync<ShipmentResponse>(
                $"/api/InboundShipment/{shipmentId}");

        shipment.ShouldNotBeNull();
        shipment.Status.ShouldBe("Received");
        shipment.Lines.Single().ReceivedQuantity.ShouldBe(40);
        shipment.Deliveries.ShouldNotBeNull();
        shipment.Deliveries.Count.ShouldBe(2);
        shipment.Deliveries.ShouldAllBe(
            x => x.Status == "Received");

        shipment.Receipts.ShouldNotBeNull();
        shipment.Receipts.Count.ShouldBe(2);
        shipment.Receipts.ShouldContain(
            x =>
                x.WarehouseId == warehouseAId &&
                x.DeliveryId == deliveryAId &&
                x.Quantity == 15);
        shipment.Receipts.ShouldContain(
            x =>
                x.WarehouseId == warehouseBId &&
                x.DeliveryId == deliveryBId &&
                x.Quantity == 25);

        var stockA =
            await JourneyTestData.GetStockAsync(
                client,
                warehouseAId,
                productVarietyId);

        stockA.OnHand.ShouldBe(15);
        stockA.CountryOfOrigin.ShouldBe("ZA");

        var stockB =
            await JourneyTestData.GetStockAsync(
                client,
                warehouseBId,
                productVarietyId);

        stockB.OnHand.ShouldBe(25);
        stockB.CountryOfOrigin.ShouldBe("ZA");
    }

    private static async Task<Guid> CreateDeliveryAsync(
        HttpClient client,
        Guid shipmentId,
        Guid warehouseId,
        Guid lineId,
        int quantity,
        string vehicleNumber)
    {
        var response =
            await client.PostAsJsonAsync(
                $"/api/InboundShipment/{shipmentId}/warehouse-deliveries",
                new CreateWarehouseDeliveryRequest(
                    warehouseId,
                    "Fruit Logistics",
                    "Test Driver",
                    "+27 82 000 0000",
                    vehicleNumber,
                    "Cape Town Port",
                    $"Warehouse {warehouseId}",
                    $"DLV-{Guid.NewGuid():N}",
                    null,
                    [
                        new CreateWarehouseDeliveryAllocationRequest(
                            lineId,
                            quantity)
                    ]));

        response.StatusCode.ShouldBe(
            HttpStatusCode.Created);

        var result =
            await response.Content
                .ReadFromJsonAsync<DeliveryCreatedResponse>();

        result.ShouldNotBeNull();
        return result.Id;
    }

    private sealed record DeliveryCreatedResponse(Guid Id);
}
