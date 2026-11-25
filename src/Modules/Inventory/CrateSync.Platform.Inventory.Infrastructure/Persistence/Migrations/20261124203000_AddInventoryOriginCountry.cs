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

        // Resolve only balances whose entire positive stock history has one
        // proven origin. Unknown inputs and mixed origins must not be relabelled.
        migrationBuilder.Sql("""
            CREATE TABLE #OriginInputs (
                BalanceId uniqueidentifier NOT NULL,
                OriginCountry nvarchar(2) NULL,
                SourceBalanceId uniqueidentifier NULL
            );

            INSERT INTO #OriginInputs (BalanceId, OriginCountry, SourceBalanceId)
            SELECT b.Id,
                CASE WHEN m.Type = 1 AND s.OriginCountry <> 'ZZ'
                     AND LEN(s.OriginCountry) = 2
                     THEN UPPER(s.OriginCountry) END,
                CASE WHEN m.Type = 7 AND transfer.SourceCount = 1
                     THEN transfer.SourceBalanceId END
            FROM inventory.InventoryBalances b
            INNER JOIN inventory.StockMovements m ON m.InventoryBalanceId = b.Id
            LEFT JOIN logistics.ShipmentReceipts r
                ON m.Type = 1 AND m.ReferenceType = 'ShipmentReceipt'
                AND m.ReferenceId = r.Id AND r.WarehouseId = b.WarehouseId
                AND r.ProductVarietyId = b.ProductVarietyId
                AND r.Quantity = m.OnHandChange
            LEFT JOIN logistics.InboundShipments s
                ON s.Id = r.ShipmentId AND s.TenantId = b.TenantId
            OUTER APPLY (
                SELECT COUNT(*) AS SourceCount,
                    MIN(CONVERT(char(36), source.Id)) AS SourceBalanceId
                FROM inventory.StockMovements outgoing
                INNER JOIN inventory.InventoryBalances source
                    ON source.Id = outgoing.InventoryBalanceId
                WHERE m.Type = 7 AND m.ReferenceType = 'WarehouseTransfer'
                    AND outgoing.Type = 6
                    AND outgoing.ReferenceType = m.ReferenceType
                    AND outgoing.ReferenceId = m.ReferenceId
                    AND outgoing.OnHandChange = -m.OnHandChange
                    AND source.TenantId = b.TenantId
                    AND source.ProductVarietyId = b.ProductVarietyId
                    AND source.WarehouseId <> b.WarehouseId
            ) transfer
            WHERE m.OnHandChange > 0;

            -- Missing audit history is itself an unknown input.
            INSERT INTO #OriginInputs (BalanceId, OriginCountry, SourceBalanceId)
            SELECT b.Id, NULL, NULL
            FROM inventory.InventoryBalances b
            OUTER APPLY (
                SELECT SUM(CONVERT(bigint, m.OnHandChange)) AS TotalChange
                FROM inventory.StockMovements m WHERE m.InventoryBalanceId = b.Id
            ) history
            WHERE COALESCE(history.TotalChange, 0) <> b.OnHand;

            -- Fixed-point propagation: a transfer is proven only once its
            -- source balance is proven. Cycles without sufficient evidence
            -- and unidentified positive adjustments remain unknown.
            WHILE 1 = 1
            BEGIN
                UPDATE b
                SET b.OriginCountry = evidence.OriginCountry
                FROM inventory.InventoryBalances b
                CROSS APPLY (
                    SELECT COUNT(*) AS InputCount,
                        COUNT(COALESCE(i.OriginCountry, source.OriginCountry)) AS KnownCount,
                        COUNT(DISTINCT COALESCE(i.OriginCountry, source.OriginCountry)) AS OriginCount,
                        MAX(COALESCE(i.OriginCountry, source.OriginCountry)) AS OriginCountry
                    FROM #OriginInputs i
                    LEFT JOIN inventory.InventoryBalances source ON source.Id = i.SourceBalanceId
                    WHERE i.BalanceId = b.Id
                ) evidence
                WHERE b.OriginCountry IS NULL
                    AND evidence.InputCount > 0
                    AND evidence.InputCount = evidence.KnownCount
                    AND evidence.OriginCount = 1;

                IF @@ROWCOUNT = 0 BREAK;
            END;

            UPDATE inventory.InventoryBalances
            SET OriginCountry = 'ZZ'
            WHERE OriginCountry IS NULL;

            DROP TABLE #OriginInputs;
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
