using CrateSync.Platform.BuildingBlocks.Application.Persistence;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.GetShipment;
using CrateSync.Platform.InboundLogistics.Application.Shipments.SearchShipments;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using Dapper;

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Read;

internal sealed class InboundLogisticsReadService(ISqlConnectionFactory connectionFactory) : IInboundLogisticsReadService
{
    private readonly ISqlConnectionFactory _connectionFactory = connectionFactory;

    public async Task<ShipmentResponse?> GetByIdAsync(
        Guid tenantId,
        Guid shipmentId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                SupplierId,
                TrackingNumber,
                OriginCountry,
                CASE Status
                    WHEN 1 THEN 'Draft'
                    WHEN 2 THEN 'Shipped'
                    WHEN 3 THEN 'Arrived'
                    WHEN 4 THEN 'Receiving'
                    WHEN 5 THEN 'PartiallyReceived'
                    WHEN 6 THEN 'Received'
                    WHEN 7 THEN 'Cancelled'
                END AS Status,
                ExpectedArrivalAtUtc,
                ShippedAtUtc,
                ArrivedAtUtc,
                ReceivedAtUtc,
                CancelledAtUtc,
                CreatedAtUtc,
                UpdatedAtUtc,
                ContainerNumber,
                BillOfLadingNumber,
                PortOfLoading,
                PortOfDischarge,
                VesselName,
                VoyageNumber,
                CustomsBrokerName,
                BillOfEntryNumber,
                BillOfEntryDate,
                OutOfChargeAtUtc,
                CASE CustomsStatus
                    WHEN 1 THEN 'NotStarted'
                    WHEN 2 THEN 'BillOfEntryFiled'
                    WHEN 3 THEN 'Assessment'
                    WHEN 4 THEN 'Examination'
                    WHEN 5 THEN 'QueryRaised'
                    WHEN 6 THEN 'Held'
                    WHEN 7 THEN 'Cleared'
                    WHEN 8 THEN 'OutOfCharge'
                    WHEN 9 THEN 'Rejected'
                END AS CustomsStatus,
                CASE PlantQuarantineStatus
                    WHEN 1 THEN 'NotRequired'
                    WHEN 2 THEN 'Pending'
                    WHEN 3 THEN 'DocumentsSubmitted'
                    WHEN 4 THEN 'InspectionScheduled'
                    WHEN 5 THEN 'UnderInspection'
                    WHEN 6 THEN 'Sampling'
                    WHEN 7 THEN 'TreatmentRequired'
                    WHEN 8 THEN 'Cleared'
                    WHEN 9 THEN 'Rejected'
                END AS PlantQuarantineStatus,
                CASE FoodSafetyStatus
                    WHEN 1 THEN 'NotRequired'
                    WHEN 2 THEN 'Pending'
                    WHEN 3 THEN 'Referred'
                    WHEN 4 THEN 'DocumentScrutiny'
                    WHEN 5 THEN 'InspectionScheduled'
                    WHEN 6 THEN 'VisualInspection'
                    WHEN 7 THEN 'Sampling'
                    WHEN 8 THEN 'LabTesting'
                    WHEN 9 THEN 'NocIssued'
                    WHEN 10 THEN 'NonConformance'
                    WHEN 11 THEN 'Rejected'
                END AS FoodSafetyStatus
            FROM logistics.InboundShipments
            WHERE Id = @ShipmentId
              AND TenantId = @TenantId;

            SELECT
                Id,
                ProductId,
                ProductVarietyId,
                BatchCode,
                ExpectedQuantity,
                ReceivedQuantity,
                PackSize AS PackSizeKg,
                UnitCost,
                Currency
            FROM logistics.InboundShipmentLines
            WHERE ShipmentId = @ShipmentId
            ORDER BY Id;

            SELECT
                Id,
                CASE Authority
                    WHEN 1 THEN 'Customs'
                    WHEN 2 THEN 'PlantQuarantine'
                    WHEN 3 THEN 'FoodSafety'
                    WHEN 4 THEN 'Port'
                    WHEN 5 THEN 'Other'
                END AS Authority,
                Status,
                ReferenceNumber,
                Remarks,
                OccurredAtUtc
            FROM logistics.ShipmentClearanceEvents
            WHERE ShipmentId = @ShipmentId
            ORDER BY OccurredAtUtc DESC;

            SELECT
                Id,
                CASE Type
                    WHEN 1 THEN 'BillOfLading'
                    WHEN 2 THEN 'CommercialInvoice'
                    WHEN 3 THEN 'PackingList'
                    WHEN 4 THEN 'BillOfEntry'
                    WHEN 5 THEN 'CertificateOfOrigin'
                    WHEN 6 THEN 'PhytosanitaryCertificate'
                    WHEN 7 THEN 'ImportPermit'
                    WHEN 8 THEN 'FoodSafetyNoc'
                    WHEN 9 THEN 'PlantQuarantineRelease'
                    WHEN 10 THEN 'CustomsOutOfCharge'
                    WHEN 11 THEN 'TreatmentCertificate'
                    WHEN 12 THEN 'Other'
                END AS Type,
                DocumentName,
                ReferenceNumber,
                Notes,
                CreatedAtUtc
            FROM logistics.ShipmentDocuments
            WHERE ShipmentId = @ShipmentId
            ORDER BY CreatedAtUtc DESC;

            SELECT
                Id,
                ShipmentLineId,
                WarehouseId,
                ProductVarietyId,
                Quantity,
                ReceivedAtUtc
            FROM logistics.ShipmentReceipts
            WHERE ShipmentId = @ShipmentId
            ORDER BY ReceivedAtUtc DESC;
            """;

        await using var connection =
            await _connectionFactory.OpenConnectionAsync(cancellationToken);

        using var result = await connection.QueryMultipleAsync(
            new CommandDefinition(
                sql,
                new
                {
                    ShipmentId = shipmentId,
                    TenantId = tenantId
                },
                cancellationToken: cancellationToken));

        var header = await result.ReadSingleOrDefaultAsync<ShipmentHeader>();

        if (header is null)
            return null;

        var lines = (await result.ReadAsync<ShipmentLineResponse>()).AsList();
        var clearanceEvents =
            (await result.ReadAsync<ShipmentClearanceEventResponse>()).AsList();
        var documents =
            (await result.ReadAsync<ShipmentDocumentResponse>()).AsList();
        var receipts =
            (await result.ReadAsync<ShipmentReceiptResponse>()).AsList();

        return new ShipmentResponse(
            header.Id,
            header.SupplierId,
            header.TrackingNumber,
            header.OriginCountry,
            header.Status,
            header.ExpectedArrivalAtUtc,
            header.ShippedAtUtc,
            header.ArrivedAtUtc,
            header.ReceivedAtUtc,
            header.CancelledAtUtc,
            header.CreatedAtUtc,
            header.UpdatedAtUtc,
            lines,
            header.ContainerNumber,
            header.BillOfLadingNumber,
            header.PortOfLoading,
            header.PortOfDischarge,
            header.VesselName,
            header.VoyageNumber,
            header.CustomsBrokerName,
            header.BillOfEntryNumber,
            header.BillOfEntryDate,
            header.OutOfChargeAtUtc,
            header.CustomsStatus,
            header.PlantQuarantineStatus,
            header.FoodSafetyStatus,
            clearanceEvents,
            documents,
            receipts);
    }

    public async Task<IReadOnlyCollection<ShipmentSummaryResponse>> SearchAsync(
        Guid tenantId,
        string? searchTerm,
        ShipmentStatus? status,
        Guid? supplierId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                s.Id,
                s.SupplierId,
                s.TrackingNumber,
                s.OriginCountry,
                CASE s.Status
                    WHEN 1 THEN 'Draft'
                    WHEN 2 THEN 'Shipped'
                    WHEN 3 THEN 'Arrived'
                    WHEN 4 THEN 'Receiving'
                    WHEN 5 THEN 'PartiallyReceived'
                    WHEN 6 THEN 'Received'
                    WHEN 7 THEN 'Cancelled'
                END AS Status,
                s.ExpectedArrivalAtUtc,
                COALESCE(SUM(l.ExpectedQuantity), 0) AS TotalExpectedCrates,
                COALESCE(SUM(l.ReceivedQuantity), 0) AS TotalReceivedCrates,
                s.ContainerNumber,
                s.BillOfLadingNumber,
                s.BillOfEntryNumber,
                CASE s.CustomsStatus
                    WHEN 1 THEN 'NotStarted'
                    WHEN 2 THEN 'BillOfEntryFiled'
                    WHEN 3 THEN 'Assessment'
                    WHEN 4 THEN 'Examination'
                    WHEN 5 THEN 'QueryRaised'
                    WHEN 6 THEN 'Held'
                    WHEN 7 THEN 'Cleared'
                    WHEN 8 THEN 'OutOfCharge'
                    WHEN 9 THEN 'Rejected'
                END AS CustomsStatus,
                CASE s.PlantQuarantineStatus
                    WHEN 1 THEN 'NotRequired'
                    WHEN 2 THEN 'Pending'
                    WHEN 3 THEN 'DocumentsSubmitted'
                    WHEN 4 THEN 'InspectionScheduled'
                    WHEN 5 THEN 'UnderInspection'
                    WHEN 6 THEN 'Sampling'
                    WHEN 7 THEN 'TreatmentRequired'
                    WHEN 8 THEN 'Cleared'
                    WHEN 9 THEN 'Rejected'
                END AS PlantQuarantineStatus,
                CASE s.FoodSafetyStatus
                    WHEN 1 THEN 'NotRequired'
                    WHEN 2 THEN 'Pending'
                    WHEN 3 THEN 'Referred'
                    WHEN 4 THEN 'DocumentScrutiny'
                    WHEN 5 THEN 'InspectionScheduled'
                    WHEN 6 THEN 'VisualInspection'
                    WHEN 7 THEN 'Sampling'
                    WHEN 8 THEN 'LabTesting'
                    WHEN 9 THEN 'NocIssued'
                    WHEN 10 THEN 'NonConformance'
                    WHEN 11 THEN 'Rejected'
                END AS FoodSafetyStatus
            FROM logistics.InboundShipments s
            LEFT JOIN logistics.InboundShipmentLines l
                ON l.ShipmentId = s.Id
            WHERE s.TenantId = @TenantId
              AND (@SearchTerm IS NULL
                   OR s.TrackingNumber LIKE '%' + @SearchTerm + '%'
                   OR s.ContainerNumber LIKE '%' + @SearchTerm + '%'
                   OR s.BillOfLadingNumber LIKE '%' + @SearchTerm + '%'
                   OR s.BillOfEntryNumber LIKE '%' + @SearchTerm + '%')
              AND (@Status IS NULL OR s.Status = @Status)
              AND (@SupplierId IS NULL OR s.SupplierId = @SupplierId)
            GROUP BY
                s.Id,
                s.SupplierId,
                s.TrackingNumber,
                s.OriginCountry,
                s.Status,
                s.ExpectedArrivalAtUtc,
                s.ContainerNumber,
                s.BillOfLadingNumber,
                s.BillOfEntryNumber,
                s.CustomsStatus,
                s.PlantQuarantineStatus,
                s.FoodSafetyStatus
            ORDER BY s.ExpectedArrivalAtUtc DESC;
            """;

        await using var connection =
            await _connectionFactory.OpenConnectionAsync(cancellationToken);

        var shipments = await connection.QueryAsync<ShipmentSummaryResponse>(
            new CommandDefinition(
                sql,
                new
                {
                    TenantId = tenantId,
                    SearchTerm = string.IsNullOrWhiteSpace(searchTerm)
                        ? null
                        : searchTerm.Trim(),
                    Status = (int?)status,
                    SupplierId = supplierId
                },
                cancellationToken: cancellationToken));

        return shipments.AsList();
    }

    private sealed record ShipmentHeader(
        Guid Id,
        Guid SupplierId,
        string TrackingNumber,
        string OriginCountry,
        string Status,
        DateTimeOffset ExpectedArrivalAtUtc,
        DateTimeOffset? ShippedAtUtc,
        DateTimeOffset? ArrivedAtUtc,
        DateTimeOffset? ReceivedAtUtc,
        DateTimeOffset? CancelledAtUtc,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc,
        string? ContainerNumber,
        string? BillOfLadingNumber,
        string? PortOfLoading,
        string? PortOfDischarge,
        string? VesselName,
        string? VoyageNumber,
        string? CustomsBrokerName,
        string? BillOfEntryNumber,
        DateTimeOffset? BillOfEntryDate,
        DateTimeOffset? OutOfChargeAtUtc,
        string CustomsStatus,
        string PlantQuarantineStatus,
        string FoodSafetyStatus);
}
