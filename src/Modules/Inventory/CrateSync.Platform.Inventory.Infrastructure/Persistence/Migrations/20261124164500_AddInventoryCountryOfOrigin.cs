using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrateSync.Platform.Inventory.Infrastructure.Persistence.Migrations;

public partial class AddInventoryCountryOfOrigin : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CountryOfOrigin",
            schema: "inventory",
            table: "InventoryBalances",
            type: "nvarchar(2)",
            maxLength: 2,
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE b
            SET CountryOfOrigin = origin.OriginCountry
            FROM inventory.InventoryBalances b
            OUTER APPLY
            (
                SELECT TOP (1)
                    s.OriginCountry
                FROM inventory.StockMovements m
                INNER JOIN logistics.ShipmentReceipts r
                    ON m.ReferenceType = 'ShipmentReceipt'
                   AND m.ReferenceId = r.Id
                INNER JOIN logistics.InboundShipments s
                    ON s.Id = r.ShipmentId
                WHERE m.InventoryBalanceId = b.Id
                ORDER BY m.OccurredAtUtc
            ) origin
            WHERE origin.OriginCountry IS NOT NULL;
            """);

        migrationBuilder.Sql(
            """
            UPDATE inventory.InventoryBalances
            SET CountryOfOrigin = 'ZZ'
            WHERE CountryOfOrigin IS NULL;
            """);

        migrationBuilder.AlterColumn<string>(
            name: "CountryOfOrigin",
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
