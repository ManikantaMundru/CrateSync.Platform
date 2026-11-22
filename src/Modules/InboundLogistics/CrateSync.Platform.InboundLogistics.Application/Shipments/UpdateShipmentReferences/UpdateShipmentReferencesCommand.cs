using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.UpdateShipmentReferences;

public sealed record UpdateShipmentReferencesCommand(
    Guid ShipmentId,
    string? ContainerNumber,
    string? BillOfLadingNumber,
    string? PortOfLoading,
    string? PortOfDischarge,
    string? VesselName,
    string? VoyageNumber,
    string? CustomsBrokerName,
    string? BillOfEntryNumber,
    DateTimeOffset? BillOfEntryDate,
    DateTimeOffset? OutOfChargeAtUtc)
    : ICommand<Result>;
