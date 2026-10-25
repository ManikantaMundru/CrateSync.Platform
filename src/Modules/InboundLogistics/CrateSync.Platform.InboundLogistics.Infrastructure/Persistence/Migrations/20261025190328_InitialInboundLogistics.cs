using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialInboundLogistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "logistics");

            migrationBuilder.CreateTable(
                name: "InboundShipments",
                schema: "logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginCountry = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    ExpectedArrivalAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ShippedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ArrivedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboundShipments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InboundShipmentLines",
                schema: "logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVarietyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpectedQuantity = table.Column<int>(type: "int", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "int", nullable: false),
                    PackSize = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboundShipmentLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InboundShipmentLines_InboundShipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalSchema: "logistics",
                        principalTable: "InboundShipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboundShipmentLines_BatchCode",
                schema: "logistics",
                table: "InboundShipmentLines",
                column: "BatchCode");

            migrationBuilder.CreateIndex(
                name: "IX_InboundShipmentLines_ProductVarietyId",
                schema: "logistics",
                table: "InboundShipmentLines",
                column: "ProductVarietyId");

            migrationBuilder.CreateIndex(
                name: "IX_InboundShipmentLines_ShipmentId",
                schema: "logistics",
                table: "InboundShipmentLines",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_InboundShipments_TenantId_ExpectedArrivalAtUtc",
                schema: "logistics",
                table: "InboundShipments",
                columns: new[] { "TenantId", "ExpectedArrivalAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_InboundShipments_TenantId_Status",
                schema: "logistics",
                table: "InboundShipments",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_InboundShipments_TenantId_SupplierId",
                schema: "logistics",
                table: "InboundShipments",
                columns: new[] { "TenantId", "SupplierId" });

            migrationBuilder.CreateIndex(
                name: "IX_InboundShipments_TenantId_TrackingNumber",
                schema: "logistics",
                table: "InboundShipments",
                columns: new[] { "TenantId", "TrackingNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboundShipments_TenantId_WarehouseId",
                schema: "logistics",
                table: "InboundShipments",
                columns: new[] { "TenantId", "WarehouseId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboundShipmentLines",
                schema: "logistics");

            migrationBuilder.DropTable(
                name: "InboundShipments",
                schema: "logistics");
        }
    }
}
