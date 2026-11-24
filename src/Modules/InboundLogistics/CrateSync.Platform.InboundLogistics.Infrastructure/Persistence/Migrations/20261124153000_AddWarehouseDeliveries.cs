using System;
using CrateSync.Platform.InboundLogistics.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Migrations;

[DbContext(typeof(InboundLogisticsDbContext))]
[Migration("20261124153000_AddWarehouseDeliveries")]
public partial class AddWarehouseDeliveries : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "DeliveryId",
            schema: "logistics",
            table: "ShipmentReceipts",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "ShipmentDeliveries",
            schema: "logistics",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TransportCompany = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                DriverName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                DriverPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                VehicleNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                OriginLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                DestinationLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                TransportReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Status = table.Column<int>(type: "int", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                DispatchedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                ReceivedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShipmentDeliveries", x => x.Id);
                table.ForeignKey(
                    name: "FK_ShipmentDeliveries_InboundShipments_ShipmentId",
                    column: x => x.ShipmentId,
                    principalSchema: "logistics",
                    principalTable: "InboundShipments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ShipmentDeliveryAllocations",
            schema: "logistics",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DeliveryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ShipmentLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShipmentDeliveryAllocations", x => x.Id);
                table.ForeignKey(
                    name: "FK_ShipmentDeliveryAllocations_ShipmentDeliveries_DeliveryId",
                    column: x => x.DeliveryId,
                    principalSchema: "logistics",
                    principalTable: "ShipmentDeliveries",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ShipmentReceipts_DeliveryId",
            schema: "logistics",
            table: "ShipmentReceipts",
            column: "DeliveryId");

        migrationBuilder.CreateIndex(
            name: "IX_ShipmentDeliveries_ShipmentId",
            schema: "logistics",
            table: "ShipmentDeliveries",
            column: "ShipmentId");

        migrationBuilder.CreateIndex(
            name: "IX_ShipmentDeliveries_WarehouseId",
            schema: "logistics",
            table: "ShipmentDeliveries",
            column: "WarehouseId");

        migrationBuilder.CreateIndex(
            name: "IX_ShipmentDeliveries_Status",
            schema: "logistics",
            table: "ShipmentDeliveries",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_ShipmentDeliveryAllocations_DeliveryId",
            schema: "logistics",
            table: "ShipmentDeliveryAllocations",
            column: "DeliveryId");

        migrationBuilder.CreateIndex(
            name: "IX_ShipmentDeliveryAllocations_ShipmentLineId",
            schema: "logistics",
            table: "ShipmentDeliveryAllocations",
            column: "ShipmentLineId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ShipmentDeliveryAllocations",
            schema: "logistics");

        migrationBuilder.DropTable(
            name: "ShipmentDeliveries",
            schema: "logistics");

        migrationBuilder.DropIndex(
            name: "IX_ShipmentReceipts_DeliveryId",
            schema: "logistics",
            table: "ShipmentReceipts");

        migrationBuilder.DropColumn(
            name: "DeliveryId",
            schema: "logistics",
            table: "ShipmentReceipts");
    }
}
