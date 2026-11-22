using CrateSync.Platform.InboundLogistics.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Migrations;

[DbContext(typeof(InboundLogisticsDbContext))]
[Migration("20261122193500_AddShipmentComplianceTracking")]
public partial class AddShipmentComplianceTracking : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ContainerNumber",
            schema: "logistics",
            table: "InboundShipments",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "BillOfLadingNumber",
            schema: "logistics",
            table: "InboundShipments",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PortOfLoading",
            schema: "logistics",
            table: "InboundShipments",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PortOfDischarge",
            schema: "logistics",
            table: "InboundShipments",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "VesselName",
            schema: "logistics",
            table: "InboundShipments",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "VoyageNumber",
            schema: "logistics",
            table: "InboundShipments",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "CustomsBrokerName",
            schema: "logistics",
            table: "InboundShipments",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "BillOfEntryNumber",
            schema: "logistics",
            table: "InboundShipments",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "BillOfEntryDate",
            schema: "logistics",
            table: "InboundShipments",
            type: "datetimeoffset",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "OutOfChargeAtUtc",
            schema: "logistics",
            table: "InboundShipments",
            type: "datetimeoffset",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "CustomsStatus",
            schema: "logistics",
            table: "InboundShipments",
            type: "int",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.AddColumn<int>(
            name: "PlantQuarantineStatus",
            schema: "logistics",
            table: "InboundShipments",
            type: "int",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.AddColumn<int>(
            name: "FoodSafetyStatus",
            schema: "logistics",
            table: "InboundShipments",
            type: "int",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.CreateTable(
            name: "ShipmentClearanceEvents",
            schema: "logistics",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Authority = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShipmentClearanceEvents", x => x.Id);
                table.ForeignKey(
                    name: "FK_ShipmentClearanceEvents_InboundShipments_ShipmentId",
                    column: x => x.ShipmentId,
                    principalSchema: "logistics",
                    principalTable: "InboundShipments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ShipmentDocuments",
            schema: "logistics",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                DocumentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShipmentDocuments", x => x.Id);
                table.ForeignKey(
                    name: "FK_ShipmentDocuments_InboundShipments_ShipmentId",
                    column: x => x.ShipmentId,
                    principalSchema: "logistics",
                    principalTable: "InboundShipments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ShipmentClearanceEvents_Authority_OccurredAtUtc",
            schema: "logistics",
            table: "ShipmentClearanceEvents",
            columns: new[] { "Authority", "OccurredAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_ShipmentClearanceEvents_ShipmentId",
            schema: "logistics",
            table: "ShipmentClearanceEvents",
            column: "ShipmentId");

        migrationBuilder.CreateIndex(
            name: "IX_ShipmentDocuments_ShipmentId",
            schema: "logistics",
            table: "ShipmentDocuments",
            column: "ShipmentId");

        migrationBuilder.CreateIndex(
            name: "IX_ShipmentDocuments_Type",
            schema: "logistics",
            table: "ShipmentDocuments",
            column: "Type");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ShipmentClearanceEvents",
            schema: "logistics");

        migrationBuilder.DropTable(
            name: "ShipmentDocuments",
            schema: "logistics");

        foreach (var column in new[]
        {
            "ContainerNumber",
            "BillOfLadingNumber",
            "PortOfLoading",
            "PortOfDischarge",
            "VesselName",
            "VoyageNumber",
            "CustomsBrokerName",
            "BillOfEntryNumber",
            "BillOfEntryDate",
            "OutOfChargeAtUtc",
            "CustomsStatus",
            "PlantQuarantineStatus",
            "FoodSafetyStatus"
        })
        {
            migrationBuilder.DropColumn(
                name: column,
                schema: "logistics",
                table: "InboundShipments");
        }
    }
}
