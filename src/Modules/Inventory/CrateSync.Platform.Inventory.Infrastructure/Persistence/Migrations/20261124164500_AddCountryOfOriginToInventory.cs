using CrateSync.Platform.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrateSync.Platform.Inventory.Infrastructure.Persistence.Migrations;

[DbContext(typeof(InventoryDbContext))]
[Migration("20261124164500_AddCountryOfOriginToInventory")]
public partial class AddCountryOfOriginToInventory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_InventoryBalances_TenantId_WarehouseId_ProductVarietyId",
            schema: "inventory",
            table: "InventoryBalances");

        migrationBuilder.AddColumn<string>(
            name: "CountryOfOrigin",
            schema: "inventory",
            table: "InventoryBalances",
            type: "nvarchar(2)",
            maxLength: 2,
            nullable: false,
            defaultValue: "ZZ");

        migrationBuilder.Sql(
            """
            UPDATE b
            SET CountryOfOrigin = origins.OriginCountry
            FROM inventory.InventoryBalances b
            CROSS APPLY
            (
                SELECT
                    MIN(s.OriginCountry) AS OriginCountry,
                    COUNT(DISTINCT s.OriginCountry) AS OriginCount
                FROM inventory.StockMovements m
                INNER JOIN logistics.ShipmentReceipts r
                    ON m.ReferenceType = 'ShipmentReceipt'
                   AND m.ReferenceId = r.Id
                INNER JOIN logistics.InboundShipments s
                    ON s.Id = r.ShipmentId
                WHERE m.InventoryBalanceId = b.Id
            ) origins
            WHERE origins.OriginCount = 1;
            """);

        migrationBuilder.CreateIndex(
            name: "IX_InventoryBalances_TenantId_CountryOfOrigin",
            schema: "inventory",
            table: "InventoryBalances",
            columns: new[] { "TenantId", "CountryOfOrigin" });

        migrationBuilder.CreateIndex(
            name: "IX_InventoryBalances_TenantId_WarehouseId_ProductVarietyId_CountryOfOrigin",
            schema: "inventory",
            table: "InventoryBalances",
            columns: new[]
            {
                "TenantId",
                "WarehouseId",
                "ProductVarietyId",
                "CountryOfOrigin"
            },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_InventoryBalances_TenantId_CountryOfOrigin",
            schema: "inventory",
            table: "InventoryBalances");

        migrationBuilder.DropIndex(
            name: "IX_InventoryBalances_TenantId_WarehouseId_ProductVarietyId_CountryOfOrigin",
            schema: "inventory",
            table: "InventoryBalances");

        migrationBuilder.DropColumn(
            name: "CountryOfOrigin",
            schema: "inventory",
            table: "InventoryBalances");

        migrationBuilder.CreateIndex(
            name: "IX_InventoryBalances_TenantId_WarehouseId_ProductVarietyId",
            schema: "inventory",
            table: "InventoryBalances",
            columns: new[] { "TenantId", "WarehouseId", "ProductVarietyId" },
            unique: true);
    }
}
