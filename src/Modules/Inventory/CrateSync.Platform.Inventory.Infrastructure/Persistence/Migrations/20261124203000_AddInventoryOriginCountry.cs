using CrateSync.Platform.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrateSync.Platform.Inventory.Infrastructure.Persistence.Migrations;

[DbContext(typeof(InventoryDbContext))]
[Migration("20261124203000_AddInventoryOriginCountry")]
public partial class AddInventoryOriginCountry : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "OriginCountry",
            schema: "inventory",
            table: "InventoryBalances",
            type: "nvarchar(2)",
            maxLength: 2,
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE b
            SET b.OriginCountry =
                CASE
                    WHEN x.OriginCount = 1 THEN x.OriginCountry
                    ELSE 'ZZ'
                END
            FROM inventory.InventoryBalances b
            OUTER APPLY (
                SELECT
                    COUNT(DISTINCT s.OriginCountry) AS OriginCount,
                    MAX(s.OriginCountry) AS OriginCountry
                FROM inventory.StockMovements m
                INNER JOIN logistics.ShipmentReceipts r
                    ON m.ReferenceType = 'ShipmentReceipt'
                   AND m.ReferenceId = r.Id
                INNER JOIN logistics.InboundShipments s
                    ON s.Id = r.ShipmentId
                WHERE m.InventoryBalanceId = b.Id
                  AND m.Type = 1
            ) x;
            """);

        migrationBuilder.Sql("""
            UPDATE inventory.InventoryBalances
            SET OriginCountry = 'ZZ'
            WHERE OriginCountry IS NULL;
            """);

        migrationBuilder.AlterColumn<string>(
            name: "OriginCountry",
            schema: "inventory",
            table: "InventoryBalances",
            type: "nvarchar(2)",
            maxLength: 2,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(2)",
            oldMaxLength: 2,
            oldNullable: true);

        migrationBuilder.DropIndex(
            name: "IX_InventoryBalances_TenantId_WarehouseId_ProductVarietyId",
            schema: "inventory",
            table: "InventoryBalances");

        migrationBuilder.CreateIndex(
            name: "IX_InventoryBalances_TenantId_WarehouseId_ProductVarietyId_OriginCountry",
            schema: "inventory",
            table: "InventoryBalances",
            columns: new[] { "TenantId", "WarehouseId", "ProductVarietyId", "OriginCountry" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_InventoryBalances_TenantId_WarehouseId_ProductVarietyId_OriginCountry",
            schema: "inventory",
            table: "InventoryBalances");

        migrationBuilder.CreateIndex(
            name: "IX_InventoryBalances_TenantId_WarehouseId_ProductVarietyId",
            schema: "inventory",
            table: "InventoryBalances",
            columns: new[] { "TenantId", "WarehouseId", "ProductVarietyId" },
            unique: true);

        migrationBuilder.DropColumn(
            name: "OriginCountry",
            schema: "inventory",
            table: "InventoryBalances");
    }
}
