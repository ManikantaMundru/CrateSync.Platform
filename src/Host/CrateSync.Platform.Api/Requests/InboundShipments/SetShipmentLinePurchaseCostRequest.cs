namespace CrateSync.Platform.Api.Requests.InboundShipments;

public sealed record SetShipmentLinePurchaseCostRequest(decimal UnitCost, string Currency);
