using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace CrateSync.Platform.Inventory.Infrastructure.Persistence.Migrations;
public partial class AddStockAgeing : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(name: "OriginalReceivedAtUtc", schema: "inventory", table: "InventoryBalances", type: "datetimeoffset", nullable: true);
        migrationBuilder.AddColumn<DateTimeOffset>(name: "OriginalReceivedAtUtc", schema: "inventory", table: "WarehouseTransfers", type: "datetimeoffset", nullable: true);
        // A classified batch is a shipment line. Use its earliest actual accepted receipt across warehouses.
        // Do not infer dates for legacy mixed balances or batches without receipt evidence.
        migrationBuilder.Sql("""
            UPDATE b SET OriginalReceivedAtUtc = receipts.FirstReceipt
            FROM inventory.InventoryBalances b
            JOIN (
                SELECT s.TenantId, r.ShipmentLineId, MIN(r.ReceivedAtUtc) AS FirstReceipt
                FROM logistics.ShipmentReceipts r
                JOIN logistics.InboundShipments s ON s.Id = r.ShipmentId
                WHERE r.Quantity > 0
                GROUP BY s.TenantId, r.ShipmentLineId
            ) receipts ON receipts.TenantId = b.TenantId AND receipts.ShipmentLineId = b.BatchId
            WHERE b.BatchId <> '00000000-0000-0000-0000-000000000000';
            UPDATE t SET OriginalReceivedAtUtc = b.OriginalReceivedAtUtc
            FROM inventory.WarehouseTransfers t
            JOIN inventory.InventoryBalances b ON b.TenantId=t.TenantId AND b.WarehouseId=t.SourceWarehouseId
                AND b.ProductVarietyId=t.ProductVarietyId AND b.OriginCountry=t.OriginCountry AND b.BatchId=t.BatchId;
            """);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("OriginalReceivedAtUtc", "InventoryBalances", "inventory");
        migrationBuilder.DropColumn("OriginalReceivedAtUtc", "WarehouseTransfers", "inventory");
    }
}
