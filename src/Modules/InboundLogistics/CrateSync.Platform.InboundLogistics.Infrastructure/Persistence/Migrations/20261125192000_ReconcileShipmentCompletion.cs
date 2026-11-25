using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Migrations;

public partial class ReconcileShipmentCompletion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(name: "ReversedAtUtc", schema: "logistics", table: "RegulatorySamples", type: "datetimeoffset", nullable: true);
        migrationBuilder.AddColumn<string>(name: "ReversalReason", schema: "logistics", table: "RegulatorySamples", type: "nvarchar(500)", maxLength: 500, nullable: true);
        migrationBuilder.Sql("""
            UPDATE s
            SET Status = CASE WHEN EXISTS (
                SELECT 1 FROM logistics.ShipmentDeliveries d
                JOIN logistics.ShipmentDeliveryAllocations a ON a.DeliveryId = d.Id
                WHERE d.ShipmentId = s.Id AND d.Status = 3 AND (COALESCE(a.DamagedQuantity,0) + COALESCE(a.MissingQuantity,0)) > 0
            ) THEN 8 ELSE 6 END,
            ReceivedAtUtc = COALESCE(s.ReceivedAtUtc, (SELECT MAX(d.ReceivedAtUtc) FROM logistics.ShipmentDeliveries d WHERE d.ShipmentId = s.Id AND d.Status = 3), s.UpdatedAtUtc),
            UpdatedAtUtc = SYSDATETIMEOFFSET()
            FROM logistics.InboundShipments s
            WHERE s.Status IN (3,4,5)
              AND EXISTS (SELECT 1 FROM logistics.InboundShipmentLines l WHERE l.ShipmentId = s.Id)
              AND NOT EXISTS (SELECT 1 FROM logistics.ShipmentDeliveries d WHERE d.ShipmentId = s.Id AND d.Status IN (1,2,5))
              AND NOT EXISTS (
                SELECT 1 FROM logistics.InboundShipmentLines l WHERE l.ShipmentId = s.Id
                AND l.ExpectedQuantity <> l.ReceivedQuantity
                  + COALESCE((SELECT SUM(rs.Quantity) FROM logistics.RegulatorySamples rs WHERE rs.ShipmentId = s.Id AND rs.ShipmentLineId = l.Id AND rs.ReversedAtUtc IS NULL),0)
                  + COALESCE((SELECT SUM(COALESCE(a.DamagedQuantity,0)+COALESCE(a.MissingQuantity,0))
                    FROM logistics.ShipmentDeliveryAllocations a JOIN logistics.ShipmentDeliveries d ON d.Id = a.DeliveryId
                    WHERE d.ShipmentId = s.Id AND d.Status = 3 AND a.ShipmentLineId = l.Id),0)
              );
            """);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM logistics.RegulatorySamples WHERE ReversedAtUtc IS NOT NULL) THROW 50001, 'Sample reversals exist; rollback would lose audit history.', 1;");
        migrationBuilder.Sql("UPDATE logistics.InboundShipments SET Status = 5, ReceivedAtUtc = NULL WHERE Status = 8;");
        migrationBuilder.DropColumn(name: "ReversedAtUtc", schema: "logistics", table: "RegulatorySamples");
        migrationBuilder.DropColumn(name: "ReversalReason", schema: "logistics", table: "RegulatorySamples");
    }
}
