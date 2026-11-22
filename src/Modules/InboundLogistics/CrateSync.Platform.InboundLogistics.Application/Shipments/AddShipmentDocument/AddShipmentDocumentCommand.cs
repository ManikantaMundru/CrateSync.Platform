using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.AddShipmentDocument;

public sealed record AddShipmentDocumentCommand(
    Guid ShipmentId,
    ShipmentDocumentType Type,
    string DocumentName,
    string? ReferenceNumber,
    string? Notes)
    : ICommand<Result<Guid>>;
