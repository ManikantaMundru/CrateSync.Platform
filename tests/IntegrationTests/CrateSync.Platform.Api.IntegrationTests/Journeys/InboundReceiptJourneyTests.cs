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
    public async Task ReceivedInboundShipment_ShouldIncreaseInventoryAndExposeReceivedMovement()
    {
        // Arrange
        await factory.ResetAsync();

        using var client = factory.CreateClient();

        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productVarietyId = Guid.NewGuid();

        var createResponse =
            await client.PostAsJsonAsync(
                "/api/InboundShipment",
                new CreateInboundShipmentRequest(
                    Guid.NewGuid(),
                    warehouseId,
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

        // Act
        (await client.PatchAsync(
            $"/api/InboundShipment/{shipmentId}/ship",
            null)).EnsureSuccessStatusCode();

        (await client.PatchAsync(
            $"/api/InboundShipment/{shipmentId}/arrive",
            null)).EnsureSuccessStatusCode();

        var receive =
            await client.PostAsJsonAsync(
                $"/api/InboundShipment/{shipmentId}/receive",
                new ReceiveShipmentRequest(
                    lineId,
                    40));

        // Assert
        receive.StatusCode.ShouldBe(
            HttpStatusCode.NoContent);

        var shipment =
            await client.GetFromJsonAsync<ShipmentResponse>(
                $"/api/InboundShipment/{shipmentId}");

        shipment.ShouldNotBeNull();
        shipment.Status.ShouldBe("Received");
        shipment.Lines.Single().ReceivedQuantity.ShouldBe(40);

        var stock =
            await JourneyTestData.GetStockAsync(
                client,
                warehouseId,
                productVarietyId);

        stock.OnHand.ShouldBe(40);
        stock.Reserved.ShouldBe(0);
        stock.Available.ShouldBe(40);

        var movementsResponse =
            await client.GetAsync(
                $"/api/inventory/movements?warehouseId={warehouseId}&productVarietyId={productVarietyId}");

        movementsResponse.StatusCode.ShouldBe(
            HttpStatusCode.OK);

        var movementJson =
            await movementsResponse.Content.ReadAsStringAsync();

        movementJson.ShouldContain("Received");
        movementJson.ShouldContain(shipmentId.ToString());
    }
}
