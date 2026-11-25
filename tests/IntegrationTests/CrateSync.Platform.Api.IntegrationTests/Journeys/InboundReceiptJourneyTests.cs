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

        var beforeDelivery = await client.GetFromJsonAsync<ShipmentResponse>($"/api/InboundShipment/{shipmentId}");
        var allocationA = beforeDelivery!.Deliveries!.Single(d => d.Id == deliveryAId).Allocations.Single().Id;
        var allocationB = beforeDelivery.Deliveries!.Single(d => d.Id == deliveryBId).Allocations.Single().Id;
        var inspectionA = new[] { new { allocationId = allocationA, accepted = 12, damaged = 2, missing = 1, remarks = "Damaged and missing packages" } };
        var inspectionB = new[] { new { allocationId = allocationB, accepted = 25, damaged = 0, missing = 0, remarks = (string?)null } };
        var premature = await client.PatchAsJsonAsync(
            $"/api/InboundShipment/{shipmentId}/warehouse-deliveries/{deliveryAId}/receive", inspectionA);
        premature.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        foreach (var deliveryId in new[] { deliveryAId, deliveryBId })
            (await client.PatchAsync(
                $"/api/InboundShipment/{shipmentId}/warehouse-deliveries/{deliveryId}/deliver",
                null)).EnsureSuccessStatusCode();

        var delivered = await client.GetFromJsonAsync<ShipmentResponse>(
            $"/api/InboundShipment/{shipmentId}");
        delivered.ShouldNotBeNull();
        delivered.Deliveries!.ShouldAllBe(x => x.Status == "DeliveredToWarehouse");
        delivered.Lines.Single().ReceivedQuantity.ShouldBe(0);
        delivered.Receipts!.ShouldBeEmpty();

        (await client.PatchAsJsonAsync(
            $"/api/InboundShipment/{shipmentId}/warehouse-deliveries/{deliveryAId}/receive",
            inspectionA)).EnsureSuccessStatusCode();

        (await client.PatchAsJsonAsync(
            $"/api/InboundShipment/{shipmentId}/warehouse-deliveries/{deliveryBId}/receive",
            inspectionB)).EnsureSuccessStatusCode();

        var shipment =
            await client.GetFromJsonAsync<ShipmentResponse>(
                $"/api/InboundShipment/{shipmentId}");

        shipment.ShouldNotBeNull();
        shipment.Status.ShouldBe("ReceivedWithDiscrepancies");
        shipment.Lines.Single().AccountedQuantity.ShouldBe(40);
        shipment.Lines.Single().RemainingQuantity.ShouldBe(0);
        shipment.Lines.Single().ReceivedQuantity.ShouldBe(37);
        shipment.Deliveries.ShouldNotBeNull();
        shipment.Deliveries.Count.ShouldBe(2);
        shipment.Deliveries.ShouldAllBe(
            x => x.Status == "Received");

        var inspected = shipment.Deliveries.Single(d => d.Id == deliveryAId).Allocations.Single();
        inspected.AcceptedQuantity.ShouldBe(12);
        inspected.DamagedQuantity.ShouldBe(2);
        inspected.MissingQuantity.ShouldBe(1);
        shipment.Receipts.ShouldNotBeNull();
        shipment.Receipts.Count.ShouldBe(2);
        shipment.Receipts.ShouldContain(
            x =>
                x.WarehouseId == warehouseAId &&
                x.DeliveryId == deliveryAId &&
                x.Quantity == 12);
        shipment.Receipts.ShouldContain(
            x =>
                x.WarehouseId == warehouseBId &&
                x.DeliveryId == deliveryBId &&
                x.Quantity == 25);

        var stockA =
            await JourneyTestData.GetStockAsync(
                client,
                warehouseAId,
                productVarietyId, batchId: lineId);

        stockA.OnHand.ShouldBe(12);
        stockA.BatchId.ShouldBe(lineId);
        stockA.PackWeightKg.ShouldBe(18m);

        var stockB =
            await JourneyTestData.GetStockAsync(
                client,
                warehouseBId,
                productVarietyId);

        stockB.OnHand.ShouldBe(25);
        stockB.BatchId.ShouldBe(lineId);
        stockB.PackWeightKg.ShouldBe(18m);
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
