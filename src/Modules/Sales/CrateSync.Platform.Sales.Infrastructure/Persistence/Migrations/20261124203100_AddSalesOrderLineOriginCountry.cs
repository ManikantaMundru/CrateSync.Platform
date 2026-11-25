using CrateSync.Platform.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrateSync.Platform.Sales.Infrastructure.Persistence.Migrations;

[DbContext(typeof(SalesDbContext))]
[Migration("20261124203100_AddSalesOrderLineOriginCountry")]
public partial class AddSalesOrderLineOriginCountry : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "OriginCountry",
            schema: "sales",
            table: "SalesOrderLines",
            type: "nvarchar(2)",
            maxLength: 2,
            nullable: false,
            defaultValue: "ZZ");

        // Recover historical origin from the actual reservation, not from
        // currently available stock. Inventory migration must be applied first.
        migrationBuilder.Sql("""
            UPDATE line
            SET line.OriginCountry = evidence.OriginCountry
            FROM sales.SalesOrderLines line
            INNER JOIN sales.SalesOrders orders ON orders.Id = line.SalesOrderId
            CROSS APPLY (
                SELECT COUNT(*) AS ReservationCount,
                    MAX(b.OriginCountry) AS OriginCountry,
                    SUM(CONVERT(bigint, r.Quantity)) AS ReservedQuantity
                FROM inventory.StockReservations r
                INNER JOIN inventory.InventoryBalances b ON b.Id = r.InventoryBalanceId
                WHERE r.SalesOrderId = orders.Id
                    AND b.TenantId = orders.TenantId
                    AND b.WarehouseId = orders.WarehouseId
                    AND b.ProductVarietyId = line.ProductVarietyId
            ) evidence
            WHERE evidence.ReservationCount = 1
                AND evidence.ReservedQuantity = line.Quantity
                AND evidence.OriginCountry <> 'ZZ';
            """);

        migrationBuilder.DropIndex(
            name: "IX_SalesOrderLines_ProductVarietyId",
            schema: "sales",
            table: "SalesOrderLines");

        migrationBuilder.CreateIndex(
            name: "IX_SalesOrderLines_ProductVarietyId_OriginCountry",
            schema: "sales",
            table: "SalesOrderLines",
            columns: new[] { "ProductVarietyId", "OriginCountry" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_SalesOrderLines_ProductVarietyId_OriginCountry",
            schema: "sales",
            table: "SalesOrderLines");

        migrationBuilder.CreateIndex(
            name: "IX_SalesOrderLines_ProductVarietyId",
            schema: "sales",
            table: "SalesOrderLines",
            column: "ProductVarietyId");

        migrationBuilder.DropColumn(
            name: "OriginCountry",
            schema: "sales",
            table: "SalesOrderLines");
    }
}
