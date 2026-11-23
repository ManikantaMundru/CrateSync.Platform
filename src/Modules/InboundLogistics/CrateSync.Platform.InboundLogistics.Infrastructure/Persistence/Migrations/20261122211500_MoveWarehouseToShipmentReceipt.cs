using System;
using CrateSync.Platform.InboundLogistics.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Migrations;

[DbContext(typeof(InboundLogisticsDbContext))]
[Migration("20261122211500_MoveWarehouseToShipmentReceipt")]
public partial class MoveWarehouseToShipmentReceipt : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "WarehouseId",
            schema: "logistics",
            table: "ShipmentReceipts",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE r
            SET r.WarehouseId = s.WarehouseId
            FROM logistics.ShipmentReceipts r
            INNER JOIN logistics.InboundShipments s
                ON s.Id = r.ShipmentId;
            """);

        migrationBuilder.AlterColumn<Guid>(
            name: "WarehouseId",
            schema: "logistics",
            table: "ShipmentReceipts",
            type: "uniqueidentifier",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_ShipmentReceipts_WarehouseId",
            schema: "logistics",
            table: "ShipmentReceipts",
            column: "WarehouseId");

        migrationBuilder.DropIndex(
            name: "IX_InboundShipments_TenantId_WarehouseId",
            schema: "logistics",
            table: "InboundShipments");

        migrationBuilder.DropColumn(
            name: "WarehouseId",
            schema: "logistics",
            table: "InboundShipments");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "WarehouseId",
            schema: "logistics",
            table: "InboundShipments",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE s
            SET s.WarehouseId = x.WarehouseId
            FROM logistics.InboundShipments s
            OUTER APPLY (
                SELECT TOP 1 r.WarehouseId
                FROM logistics.ShipmentReceipts r
                WHERE r.ShipmentId = s.Id
                ORDER BY r.ReceivedAtUtc
            ) x;
            """);

        migrationBuilder.Sql("""
            UPDATE logistics.InboundShipments
            SET WarehouseId = '00000000-0000-0000-0000-000000000000'
            WHERE WarehouseId IS NULL;
            """);

        migrationBuilder.AlterColumn<Guid>(
            name: "WarehouseId",
            schema: "logistics",
            table: "InboundShipments",
            type: "uniqueidentifier",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_InboundShipments_TenantId_WarehouseId",
            schema: "logistics",
            table: "InboundShipments",
            columns: new[] { "TenantId", "WarehouseId" });

        migrationBuilder.DropIndex(
            name: "IX_ShipmentReceipts_WarehouseId",
            schema: "logistics",
            table: "ShipmentReceipts");

        migrationBuilder.DropColumn(
            name: "WarehouseId",
            schema: "logistics",
            table: "ShipmentReceipts");
    }
}
