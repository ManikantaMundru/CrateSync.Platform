namespace CrateSync.Platform.Api.Requests.InboundShipments;

public sealed record UpdateShipmentReferencesRequest(
    string? ContainerNumber,
    string? BillOfLadingNumber,
    string? PortOfLoading,
    string? PortOfDischarge,
    string? VesselName,
    string? VoyageNumber,
    string? CustomsBrokerName,
    string? BillOfEntryNumber,
    DateTimeOffset? BillOfEntryDate,
    DateTimeOffset? OutOfChargeAtUtc);
