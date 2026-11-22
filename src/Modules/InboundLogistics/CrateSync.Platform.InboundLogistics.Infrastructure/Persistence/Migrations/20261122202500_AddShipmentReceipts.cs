using System;
using CrateSync.Platform.InboundLogistics.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Migrations;

[DbContext(typeof(InboundLogisticsDbContext))]
[Migration("20261122202500_AddShipmentReceipts")]
public partial class AddShipmentReceipts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ShipmentReceipts",
            schema: "logistics",
            columns: table => new
            {
                Id = table.Column<Guid>(
                    type: "uniqueidentifier",
                    nullable: false),
                ShipmentId = table.Column<Guid>(
                    type: "uniqueidentifier",
                    nullable: false),
                ShipmentLineId = table.Column<Guid>(
                    type: "uniqueidentifier",
                    nullable: false),
                ProductVarietyId = table.Column<Guid>(
                    type: "uniqueidentifier",
                    nullable: false),
                Quantity = table.Column<int>(
                    type: "int",
                    nullable: false),
                ReceivedAtUtc = table.Column<DateTimeOffset>(
                    type: "datetimeoffset",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_ShipmentReceipts",
                    x => x.Id);

                table.ForeignKey(
                    name: "FK_ShipmentReceipts_InboundShipments_ShipmentId",
                    column: x => x.ShipmentId,
                    principalSchema: "logistics",
                    principalTable: "InboundShipments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ShipmentReceipts_ShipmentId",
            schema: "logistics",
            table: "ShipmentReceipts",
            column: "ShipmentId");

        migrationBuilder.CreateIndex(
            name: "IX_ShipmentReceipts_ShipmentLineId",
            schema: "logistics",
            table: "ShipmentReceipts",
            column: "ShipmentLineId");
    }

    protected override void Down(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ShipmentReceipts",
            schema: "logistics");
    }
}
