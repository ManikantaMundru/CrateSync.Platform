using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.RecordClearanceEvent;

public sealed record RecordClearanceEventCommand(
    Guid ShipmentId,
    ClearanceAuthority Authority,
    string Status,
    string? ReferenceNumber,
    string? Remarks)
    : ICommand<Result<Guid>>;
